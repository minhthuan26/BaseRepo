using CommonLibs.Options;
using CommonLibs.Utils;
using Gateway.Application.Interfaces;
using Gateway.Infrastructure.DBContext;
using Gateway.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            var temporaryFactory = new SqlConnectionFactory(configuration);
            string resolvedConnectionString = temporaryFactory.GetConnectionString();
            services.AddDbContext<GatewayDbContext>(options =>
            {
                options.UseSqlServer(resolvedConnectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });
            });
            services.AddScoped<JwtSecurityTokenHandler>();
            services.AddAutoMapper(cfg => { }, Assembly.Load("Gateway.Application"));
            services.AddScoped<IGatewayRepository, GatewayRepository>();
            return services;
        }
    }
}
