using Microsoft.Extensions.DependencyInjection;
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
    }
}
