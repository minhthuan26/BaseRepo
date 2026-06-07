using CommonLibs.Options;
using CommonLibs.Services;
using CommonLibs.Utils;
using Gateway.Application.Interfaces;
using Gateway.Infrastructure.DBContext;
using Gateway.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;

namespace Gateway.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //1. Đăng ký các service chung từ CommonLibs
            services.AddCommonServices<GatewayDbContext>(configuration, "Gateway.Application");

            // KÍCH HOẠT REDIS HELPER (STATIC)
            //var redisConnString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            //const string instancePrefix = "BaseRepo_";
            //RedisHelper.Initialize(redisConnString, instancePrefix);
            // Đăng ký hạ tầng REDIS ASYNC (IDistributedCache) chính chủ Microsoft
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = redisConnString;
            //    options.InstanceName = instancePrefix;
            //});
            //services.AddScoped<IRedisService, RedisService>();

            // Đăng ký dịch vụ phát tin nhắn RABBITMQ dạng SINGLETON độc nhất
            //services.AddSingleton<IRabbitMQService>(sp =>
            //{
            //    var host = configuration["RabbitMQ:HostName"] ?? "localhost";
            //    var user = configuration["RabbitMQ:UserName"] ?? "guest";
            //    var pass = configuration["RabbitMQ:Password"] ?? "guest";
            //    var port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");

            //    return new RabbitMQService(host, user, pass, port);
            //});

            // 3. Đăng ký các Repository nội bộ
            services.AddScoped<JwtSecurityTokenHandler>();
            services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // 4. Đăng ký các dịch vụ đảm bảo khởi động (EnsureServiceRunning) nếu cần thiết
            services.AddEnsureServiceRunning("GatewayServiceDependencies");

            return services;
        }
    }
}
