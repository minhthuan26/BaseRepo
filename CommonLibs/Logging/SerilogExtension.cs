using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Logging
{
    public static class SerilogExtension
    {
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";

        public static void AddSharedSerilog(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                // Enrich.FromLogContext giúp Serilog nhận diện được các thuộc tính động như CorrelationId
                .Enrich.FromLogContext()
                .Enrich.WithDemystifiedStackTraces()
                .CreateLogger();

            builder.Host.UseSerilog();
        }

        public static void UseSharedSerilogRequestLogging(this WebApplication app)
        {
            // 1. Middleware xử lý Correlation ID (Phải đặt TRƯỚC UseSerilogRequestLogging)
            app.Use(async (context, next) =>
            {
                // Kiểm tra xem Header của Request có sẵn mã chưa (do Gateway hoặc Service trước truyền sang)
                if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
                {
                    // Nếu chưa có (ví dụ tại Gateway), tự sinh mã mới
                    correlationId = Guid.NewGuid().ToString();
                }

                // Trả về Header trong Response để Client hoặc Service gọi trước có thể track ngược lại nếu cần
                context.Response.Headers[CorrelationIdHeaderKey] = correlationId;

                // Đẩy mã này vào Context của Serilog với Key là "CorrelationId"
                using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
                {
                    await next();
                }
            });

            // 2. Log request HTTP (lúc này đã có kèm CorrelationId)
            app.UseSerilogRequestLogging(options =>
            {
                // Thêm mã CorrelationId vào trực tiếp dòng log request để dễ nhìn
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms [Trace: {CorrelationId}]";
            });

            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        }
    }
}
