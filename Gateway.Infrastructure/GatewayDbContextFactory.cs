using Gateway.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using CommonLibs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Infrastructure
{
    public class GatewayDbContextFactory : IDesignTimeDbContextFactory<GatewayDbContext>
    {
        public GatewayDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GatewayDbContext>();
            string connectionString = Environment.GetEnvironmentVariable("BloodBankPro__ConnectionStrings__DefaultConnection") ?? throw new AppException("[GatewayDbContextFactory] Could not find the connection string");
            optionsBuilder.UseSqlServer(connectionString);

            return new GatewayDbContext(optionsBuilder.Options);
        }
    }
}
