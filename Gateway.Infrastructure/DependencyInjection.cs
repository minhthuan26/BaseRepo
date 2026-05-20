using Gateway.Application.Interfaces;
using Gateway.Infrastructure.DBContext;
using Gateway.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommonLibs.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

namespace Gateway.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.Load("Gateway.Application"))
            );

            services.AddDbContext<GatewayDbContext>(options =>
            {
                string connectionString = Environment.GetEnvironmentVariable("Mitalab__ConnectionStrings__DefaultConnection") ?? throw new AppException("[AddGatewayInfrastructure] Could not find the connection string");
                options.UseSqlServer(connectionString);
            });
            services.AddScoped<JwtSecurityTokenHandler>();
            services.AddAutoMapper(cfg => { }, Assembly.Load("Gateway.Application"));
            services.AddScoped<IGatewayRepository, GatewayRepository>();
            return services;
        }
    }
}
