using Site.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CommonLibs.BaseDbContext;
using Microsoft.Extensions.Logging;

namespace Site.Infrastructure.DBContext
{
    public class SiteDbContext : BaseContext
    {
        public SiteDbContext(DbContextOptions<SiteDbContext> options, ILogger<SiteDbContext> logger): base(options, logger)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
