using CommonLibs.DTOs;
using CommonLibs.Entities;
using CommonLibs.Utils;
using Dapper;
using FastMember;
using log4net.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Data;
using System.Net;

namespace CommonLibs.BaseDbContext
{
    public class BaseContext : DbContext
    {
        private readonly ILogger<BaseContext> _logger;
        public BaseContext(DbContextOptions options, ILogger<BaseContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        #region Bulk
        private static DataTable ToDataTable<T>(IEnumerable<T> data)
        {
            var table = new DataTable();

            var properties = TypeDescriptor.GetProperties(typeof(T))
                .Cast<PropertyDescriptor>()
                .Where(p => p.IsBrowsable)
                .ToArray();

            foreach (var property in properties)
            {
                var type = Nullable.GetUnderlyingType(property.PropertyType)
                           ?? property.PropertyType;

                table.Columns.Add(property.Name, type);
            }

            foreach (var item in data)
            {
                var row = table.NewRow();

                foreach (var property in properties)
                {
                    row[property.Name] =
                        property.GetValue(item) ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        public async Task ExecuteBulkAsync(
            IDataReader reader,
            string destinationTable,
            Dictionary<string, string>? columnMappings = null,
            int batchSize = 5000,
            int bulkCopyTimeout = 0,
            SqlBulkCopyOptions options = SqlBulkCopyOptions.TableLock,
            CancellationToken cancellationToken = default)
        {
            if (Connection is not SqlConnection connection)
                throw new NotSupportedException("Bulk copy only supports SQL Server.");

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            using var bulk = new SqlBulkCopy(
                connection,
                options,
                CurrentTransaction as SqlTransaction);

            bulk.DestinationTableName = destinationTable;
            bulk.BatchSize = batchSize;
            bulk.BulkCopyTimeout = bulkCopyTimeout;

            if (columnMappings != null)
            {
                foreach (var mapping in columnMappings)
                {
                    // Property -> SQL Column
                    bulk.ColumnMappings.Add(mapping.Value, mapping.Key);
                }
            }

            await bulk.WriteToServerAsync(reader, cancellationToken);
        }

        public async Task ExecuteBulkAsync<T>(
            IEnumerable<T> data,
            string destinationTable,
            int batchSize = 5000,
            int bulkCopyTimeout = 0,
            SqlBulkCopyOptions options = SqlBulkCopyOptions.TableLock,
            CancellationToken cancellationToken = default)
            where T : BaseInputDTO, new()
        {
            if (data == null)
                throw new AppException($"Can not import null data => {nameof(data)}");

            var list = data as IList<T> ?? data.ToList();

            if (list.Count == 0)
                return;

            var mappings = BaseInputDTO.GetHeaders<T>();

            using var reader = ObjectReader.Create(
                list,
                mappings.Values.ToArray());

            await ExecuteBulkAsync(
                reader,
                destinationTable,
                mappings,
                batchSize,
                bulkCopyTimeout,
                options,
                cancellationToken);
        }
        #endregion

        #region Dapper

        public IDbConnection Connection
            => Database.GetDbConnection();

        public IDbTransaction? CurrentTransaction
            => Database.CurrentTransaction?.GetDbTransaction();

        public async Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? param = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await Connection.QueryAsync<T>(
                                new CommandDefinition(
                                    sql,
                                    param,
                                    transaction: CurrentTransaction,
                                    cancellationToken: cancellationToken
                                ));
            }
            catch
            {
                throw;
            }

        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(
            string sql,
            object? param = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await Connection.QueryFirstOrDefaultAsync<T>(
                                new CommandDefinition(
                                    sql,
                                    param,
                                    transaction: CurrentTransaction,
                                    cancellationToken: cancellationToken
                                ));
            }
            catch
            {
                throw;
            }

        }

        public async Task<T> ExecuteScalarAsync<T>(
            string sql,
            object? param = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await Connection.ExecuteScalarAsync<T>(
                                new CommandDefinition(
                                    sql,
                                    param,
                                    transaction: CurrentTransaction,
                                    cancellationToken: cancellationToken
                                ));
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> ExecuteAsync(
            string sql,
            object? param = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await Connection.ExecuteAsync(
                                new CommandDefinition(
                                    sql,
                                    param,
                                    transaction: CurrentTransaction,
                                    cancellationToken: cancellationToken
                                ));
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region SaveChanges

        public override int SaveChanges()
        {
            ApplyAuditInformation();

            var result = base.SaveChanges();

            if (result < 0)
            {
                throw new AppException(
                    "Failed to save changes.",
                    HttpStatusCode.InternalServerError);
            }

            return result;
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();

            var result = await base.SaveChangesAsync(cancellationToken);

            if (result < 0)
            {
                throw new AppException(
                    "Failed to save changes.",
                    HttpStatusCode.InternalServerError);
            }

            return result;
        }

        private void ApplyAuditInformation()
        {
            var entries = ChangeTracker
                .Entries<BaseEntity>()
                .Where(x =>
                    x.State == EntityState.Added ||
                    x.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Entity.UpdatedDate = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                }
            }
        }

        #endregion

        #region Transaction

        public async Task<TResult> ExecuteWithTransactionAsync<TResult>(
            Func<Task<TResult>> action,
            bool saveChanges = true,
            CancellationToken cancellationToken = default)
        {
            await using var transaction =
                await Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await action();

                if (saveChanges)
                {
                    await SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task ExecuteWithTransactionAsync(
            Func<Task> action,
            CancellationToken cancellationToken = default)
        {
            await using var transaction =
                await Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await action();

                await SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<TResult> ExecuteWithTransactionAsync<TResult>(
            Func<Task<TResult>> action,
            CancellationToken cancellationToken = default)
        {
            await using var transaction =
                await Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await action();

                await SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        #endregion
    }
}

