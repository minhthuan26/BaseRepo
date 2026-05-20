using Gateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CommonLibs.BaseDbContext;

namespace Gateway.Infrastructure.DBContext
{
    public class GatewayDbContext : BaseContext
    {
        public GatewayDbContext(DbContextOptions<GatewayDbContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GatewayDbContext).Assembly);

            modelBuilder.Entity<Account>().HasKey(a => a.Id);
            modelBuilder.Entity<Account>().HasIndex(a => a.AccountName).IsUnique();
            modelBuilder.Entity<Account>().Property(a => a.AccountName).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Account>().Property(a => a.Password).IsRequired().HasMaxLength(255);
        }

        public DbSet<Account> Accounts { get; set; }
    }
}
