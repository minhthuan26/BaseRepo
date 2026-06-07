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
            // 1. Đăng ký các thành phần Application nội bộ của Gateway
            var applicationSection = configuration.GetSection("Application");
            var appOptions = new ApplicationOptions(); 
            applicationSection.Bind(appOptions);
            appOptions.ActualAccessToken = EnvironmentHelper.Get(appOptions.AccessToken ?? "")
            ?? throw new AppException($"Biến môi trường '{appOptions.AccessToken}' cho AccessToken bị thiếu.", HttpStatusCode.InternalServerError);
            appOptions.ActualRefreshToken = EnvironmentHelper.Get(appOptions.RefreshToken ?? "")
            ?? throw new AppException($"Biến môi trường '{appOptions.RefreshToken}' cho RefreshToken bị thiếu.", HttpStatusCode.InternalServerError);
            appOptions.ActualAudience = EnvironmentHelper.Get(appOptions.Audience ?? "")
            ?? throw new AppException($"Biến môi trường '{appOptions.Audience}' cho Audience bị thiếu.", HttpStatusCode.InternalServerError);
            appOptions.ActualIssuer = EnvironmentHelper.Get(appOptions.Issuer ?? "")
            ?? throw new AppException($"Biến môi trường '{appOptions.Issuer}' cho Issuer bị thiếu.", HttpStatusCode.InternalServerError);
            appOptions.ActualPrivateKey = EnvironmentHelper.Get(appOptions.PrivateKey ?? "")
            ?? throw new AppException($"Biến môi trường '{appOptions.PrivateKey}' cho PrivateKey bị thiếu.", HttpStatusCode.InternalServerError);
            appOptions.ActualPublicKey = EnvironmentHelper.Get(appOptions.PublicKey ?? "")
            ?? throw new AppException($"Biến môi trường '{appOptions.PublicKey}' cho PublicKey bị thiếu.", HttpStatusCode.InternalServerError);

            var applicationAssembly = Assembly.Load("Gateway.Application");
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
            services.AddAutoMapper(cfg => { }, applicationAssembly);

            // 2. Sử dụng hàm giải quyết ConnectionString và DbContext chung từ CommonLibs
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            var temporaryFactory = new SqlConnectionFactory(configuration);
            string resolvedConnectionString = temporaryFactory.GetConnectionString();
            // Gọi hàm rút gọn từ CommonLibs
            services.AddSharedDbContext<GatewayDbContext>(resolvedConnectionString);

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
            services.AddScoped<IAuditContext, AuditContext>();

            // 4. Đăng ký các dịch vụ đảm bảo khởi động (EnsureServiceRunning) nếu cần thiết
            services.AddEnsureServiceRunning("GatewayServiceDependencies");

            return services;
        }
    }
}
