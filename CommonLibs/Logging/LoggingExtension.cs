using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Logging
{
    public static class DynamicLoggingExtension
    {
        public static void AddDynamicLogging(this WebApplicationBuilder builder)
        {
            // 1. Đăng ký LoggingOptions vào DI container (Chỉ cần gọi 1 lần ở đây)
            builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection("Logging"));

            // 2. Đọc trực tiếp giá trị Provider từ file config
            var provider = builder.Configuration.GetValue<string>("Logging:Provider") ?? "Serilog";

            // 3. Quyết định thư viện sẽ được Load
            if (provider.Equals("Log4net", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddSharedLog4net();
            }
            else
            {
                builder.AddSharedSerilog(); // Default là Serilog
            }
        }

        public static void UseDynamicLogging(this WebApplication app)
        {
            // Lấy option từ DI ra để kiểm tra
            var options = app.Services.GetRequiredService<IOptions<LoggingOptions>>().Value;

            if (options.Provider.Equals("Log4net", StringComparison.OrdinalIgnoreCase))
            {
                app.UseSharedLog4netRequestLogging();
            }
            else
            {
                app.UseSharedSerilogRequestLogging();
            }
        }
    }
}
