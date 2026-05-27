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

            modelBuilder.Entity<User>().HasKey(a => a.Id);
            modelBuilder.Entity<User>().HasIndex(a => a.UserId).IsUnique();
            modelBuilder.Entity<User>().Property(a => a.UserId).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<User>().Property(a => a.Password).IsRequired().HasMaxLength(255);
        }

        public DbSet<User> Users { get; set; }
    }
}
