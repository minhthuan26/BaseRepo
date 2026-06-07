using Site.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CommonLibs.BaseDbContext;

namespace Site.Infrastructure.DBContext
{
    public class SiteDbContext : BaseContext
    {
        public SiteDbContext(DbContextOptions<SiteDbContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
