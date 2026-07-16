using CommonLibs.BaseDbContext;
using CommonLibs.Consul;
using CommonLibs.Options;
using CommonLibs.Utils;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Services
{
    public static class ServiceCollectionExtensions
    {
        private static AppOptions LoadOptions(IConfiguration configuration)
        {
            var applicationSection = configuration.GetSection("Application");

            var appOptions = new AppOptions();
            applicationSection.Bind(appOptions);

            appOptions.ActualAccessKey =
                EnvironmentHelper.Get(appOptions.AccessKey)
                ?? throw new AppException(
                    $"Biến môi trường '{appOptions.AccessKey}' cho AccessKey bị thiếu.",
                    HttpStatusCode.InternalServerError);

            appOptions.ActualRefreshKey =
                EnvironmentHelper.Get(appOptions.RefreshKey)
                ?? throw new AppException(
                    $"Biến môi trường '{appOptions.RefreshKey}' cho RefreshKey bị thiếu.",
                    HttpStatusCode.InternalServerError);

            appOptions.ActualAccessKeyByte =
                Convert.FromBase64String(appOptions.ActualAccessKey);

            appOptions.ActualRefreshKeyByte =
                Convert.FromBase64String(appOptions.ActualRefreshKey);

            appOptions.ActualAudience =
                EnvironmentHelper.Get(appOptions.Audience)
                ?? throw new AppException(
                    $"Biến môi trường '{appOptions.Audience}' cho Audience bị thiếu.",
                    HttpStatusCode.InternalServerError);

            appOptions.ActualIssuer =
                EnvironmentHelper.Get(appOptions.Issuer)
                ?? throw new AppException(
                    $"Biến môi trường '{appOptions.Issuer}' cho Issuer bị thiếu.",
                    HttpStatusCode.InternalServerError);

            appOptions.ActualPrivateKey =
                EnvironmentHelper.Get(appOptions.PrivateKey)
                ?? throw new AppException(
                    $"Biến môi trường '{appOptions.PrivateKey}' cho PrivateKey bị thiếu.",
                    HttpStatusCode.InternalServerError);

            appOptions.ActualPublicKey =
                EnvironmentHelper.Get(appOptions.PublicKey)
                ?? throw new AppException(
                    $"Biến môi trường '{appOptions.PublicKey}' cho PublicKey bị thiếu.",
                    HttpStatusCode.InternalServerError);

            return appOptions;
        }

        public static IServiceCollection AddCommonServices<TDbContext>(this IServiceCollection services, IConfiguration configuration, string assembly) where TDbContext : DbContext
        {
            // Bắt buộc phải có để HttpContextAccessor hoạt động
            services.AddHttpContextAccessor();

            // Đăng ký HttpServiceHttpClient dạng Transient hoặc Scoped
            services.AddHttpClient<HttpServiceHttpClient>();

            // Đăng ký GrpcClientHelper
            services.AddSingleton<GrpcClientHelper>();

            // Đăng ký AuditContext để thu thập thông tin ngữ cảnh cho các Repository
            services.AddScoped<IAuditContext, AuditContext>();

            // Đọc cấu hình ApplicationOptions từ appsettings.json và bind vào lớp ApplicationOptions
            var appOptions = LoadOptions(configuration);
            services.AddSingleton<IOptions<AppOptions>>(Microsoft.Extensions.Options.Options.Create(appOptions));
            if (!string.IsNullOrEmpty(appOptions.RedisConnectionString))
            {
                RedisHelper.Initialize(appOptions.RedisConnectionString, appOptions.ApplicationId);
                // 1. Đăng ký IConnectionMultiplexer (Bắt buộc là Singleton)
                // Dùng Lazy để tránh block luồng khởi động của ứng dụng nếu Redis tạm thời chưa up
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect(appOptions.RedisConnectionString));

                // 2. Đăng ký IRedisService mới đã gộp
                // Đổi từ Scoped sang Singleton vì Multiplexer là Singleton, dùng chung toàn hệ thống rất an toàn
                services.AddSingleton<IRedisService, RedisService>();
            }

            if (!string.IsNullOrEmpty(appOptions.RabbitMqConnectionString))
            {
                // Đăng ký dịch vụ phát tin nhắn RABBITMQ dạng SINGLETON độc nhất
                services.AddSingleton<IRabbitMQService>(sp =>
                {
                    return new RabbitMQService(appOptions.RabbitMqConnectionString);
                });
                AddServiceDiscovery(services);
            }

            // Đăng ký MediatR và AutoMapper, tự động quét các lớp Handler và Profile trong assembly 
            var applicationAssembly = Assembly.Load(assembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
            services.AddAutoMapper(cfg => { }, applicationAssembly);

            // Sử dụng hàm giải quyết ConnectionString và DbContext chung từ CommonLibs
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            var temporaryFactory = new SqlConnectionFactory(configuration);
            string resolvedConnectionString = temporaryFactory.GetConnectionString();
            // Gọi hàm rút gọn từ CommonLibs
            services.AddSharedDbContext<TDbContext>(resolvedConnectionString);

            return services;
        }

        /// <summary>
        /// Kích hoạt tính năng Service Discovery (Đăng ký IP/Port lên Redis và bắn sự kiện qua RabbitMQ)
        /// </summary>
        public static IServiceCollection AddServiceDiscovery(this IServiceCollection services)
        {
            // Đăng ký IHostedService chạy ngầm cùng ứng dụng
            services.AddHostedService<ServiceDiscoveryHostedService>();
            return services;
        }

        /// <summary>
        /// Kích hoạt tính năng tự động kiểm tra và khởi chạy Windows Service phụ thuộc lúc Startup
        /// </summary>
        /// <param name="sectionName">Tên key cấu hình trong appsettings.json (Mặc định là "WindowsServiceDependencies")</param>
        public static IServiceCollection AddEnsureServiceRunning(this IServiceCollection services, string sectionName = "WindowsServiceDependencies")
        {
            // Khởi tạo Hosted Service sử dụng Factory để truyền được tham số vào Constructor
            services.AddHostedService<EnsureServiceRun>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<EnsureServiceRun>>();

                return new EnsureServiceRun(configuration, logger, sectionName);
            });

            return services;
        }
    }
}
