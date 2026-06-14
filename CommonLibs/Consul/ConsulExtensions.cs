using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Consul
{
    public static class ConsulExtensions
    {
        public static IServiceCollection AddConsulRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Bind cấu hình từ appsettings.json vào object
            services.Configure<ConsulOption>(configuration.GetSection("Consul"));

            var consulOption = configuration.GetSection("Consul").Get<ConsulOption>();
            if (consulOption == null)
                throw new ArgumentNullException(nameof(ConsulOption), "Vui lòng cấu hình 'Consul' trong appsettings.json");

            // 2. Khởi tạo Singleton cho Consul Client
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(config =>
            {
                config.Address = new Uri(consulOption.Host);
            }));

            // 3. Đăng ký Hosted Service chạy ngầm
            services.AddHostedService<ConsulHostedService>();

            return services;
        }
    }
}
