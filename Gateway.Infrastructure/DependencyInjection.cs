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
using System.Reflection;

namespace Gateway.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Đăng ký các thành phần Application nội bộ của Gateway
            var applicationAssembly = Assembly.Load("Gateway.Application");
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
            services.AddAutoMapper(cfg => { }, applicationAssembly);

            // 2. Sử dụng hàm giải quyết ConnectionString và DbContext chung từ CommonLibs
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            var temporaryFactory = new SqlConnectionFactory(configuration);
            string resolvedConnectionString = temporaryFactory.GetConnectionString();

            // Gọi hàm rút gọn từ CommonLibs
            services.AddSharedDbContext<GatewayDbContext>(resolvedConnectionString);

            // Đăng ký hạ tầng REDIS chính chủ của Microsoft
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            //    options.InstanceName = "BaseRepo__";
            //});

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
            services.AddScoped<IGatewayRepository, GatewayRepository>();

            return services;
        }
    }
}
