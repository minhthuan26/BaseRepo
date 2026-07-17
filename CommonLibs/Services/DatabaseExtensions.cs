using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Services
{
    public static class DatabaseExtensions
    {
        // Hàm gom chung logic cấu hình DbContext với SQL Server cho tất cả các service
        public static IServiceCollection AddSharedDbContext<TDbContext>(this IServiceCollection services, string connectionString)
            where TDbContext : DbContext
        {
            services.AddDbContext<TDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });
            });

            return services;
        }
    }
}
