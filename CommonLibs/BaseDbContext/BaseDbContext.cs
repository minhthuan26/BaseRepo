using CommonLibs.Entities;
using CommonLibs.Utils;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Net;

namespace CommonLibs.BaseDbContext
{
    public class BaseContext : DbContext
    {
        public BaseContext(DbContextOptions options)
            : base(options)
        {
        }

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
            return await Connection.QueryAsync<T>(
                new CommandDefinition(
                    sql,
                    param,
                    transaction: CurrentTransaction,
                    cancellationToken: cancellationToken
                ));
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(
            string sql,
            object? param = null,
            CancellationToken cancellationToken = default)
        {
            return await Connection.QueryFirstOrDefaultAsync<T>(
                new CommandDefinition(
                    sql,
                    param,
                    transaction: CurrentTransaction,
                    cancellationToken: cancellationToken
                ));
        }

        public async Task<int> ExecuteAsync(
            string sql,
            object? param = null,
            CancellationToken cancellationToken = default)
        {
            return await Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    param,
                    transaction: CurrentTransaction,
                    cancellationToken: cancellationToken
                ));
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

