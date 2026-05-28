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
        }

        public DbSet<User> Users { get; set; }
    }
}
