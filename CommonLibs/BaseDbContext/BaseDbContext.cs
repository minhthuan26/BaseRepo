using CommonLibs.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.BaseDbContext
{
    public class BaseContext : DbContext
    {
        public BaseContext(DbContextOptions options) : base(options)
        {
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e =>
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                entityEntry.Property("UpdatedDate").CurrentValue = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    entityEntry.Property("CreatedDate").CurrentValue = DateTime.Now;
                }
            }

            var result = base.SaveChanges();
            if (result <= 0)
            {
                throw new AppException("Failed to save record.", HttpStatusCode.InternalServerError);
            }
            return result;
        }


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            var entries = ChangeTracker
                .Entries()
                .Where(e =>
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                entityEntry.Property("UpdatedDate").CurrentValue = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    entityEntry.Property("CreatedDate").CurrentValue = DateTime.Now;
                }
            }
            var result = await base.SaveChangesAsync(cancellationToken);
            if (result <= 0)
            {
                throw new AppException("Failed to save record.", HttpStatusCode.InternalServerError);
            }
            return result;
        }

        public override async ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = await base.AddAsync(entity, cancellationToken);
            if (result.State.ToString() != "Added")
            {
                throw new AppException("Failed to add record.", HttpStatusCode.InternalServerError);
            }
            return result;
        }

        public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        {
            var result = base.Remove(entity);
            if (result.State.ToString() != "Deleted")
            {
                throw new AppException("Failed to delete record.", HttpStatusCode.InternalServerError);
            }
            return result;
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
            var result = base.Update(entity);
            if (result.State.ToString() != "Modified")
            {
                throw new AppException("Failed to update record.", HttpStatusCode.InternalServerError);
            }
            return result;
        }

        public async Task<TEntity> ExecuteWithTransactionAsync<TEntity>(Func<Task<TEntity>> action, CancellationToken cancellationToken = default)
        {
            using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            try
            {
                TEntity result = await action();
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
    }
}
