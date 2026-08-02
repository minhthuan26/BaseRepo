using AdministrativeUnit.Domain.Entities;
using CommonLibs.BaseDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdministrativeUnit.Infrastructure.DBContext
{
    public class AdministrativeUnitDbContext : BaseContext
    {
        public AdministrativeUnitDbContext(DbContextOptions<AdministrativeUnitDbContext> options, ILogger<AdministrativeUnitDbContext> logger) : base(options, logger)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
