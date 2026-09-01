using Gateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CommonLibs.BaseDbContext;
using Microsoft.Extensions.Logging;

namespace Gateway.Infrastructure.DBContext
{
    public class GatewayDbContext : BaseContext
    {
        public GatewayDbContext(DbContextOptions<GatewayDbContext> options, ILogger<GatewayDbContext> logger): base(options, logger)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
