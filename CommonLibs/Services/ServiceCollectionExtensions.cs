using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommonHttpAndGrpcClients(this IServiceCollection services)
        {
            // Bắt buộc phải có để HttpContextAccessor hoạt động
            services.AddHttpContextAccessor();

            // Đăng ký HttpServiceHttpClient dạng Transient hoặc Scoped
            // Sử dụng AddHttpClient để .NET tự quản lý vòng đời của HttpClient qua HttpClientFactory (Tránh nghẽn Socket)
            services.AddHttpClient<HttpServiceHttpClient>();

            // Đăng ký GrpcClientHelper
            services.AddSingleton<GrpcClientHelper>();

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
