using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System.Diagnostics;
using System.Text;

namespace CommonLibs.Logging
{

    public class LocalTimeEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var localTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("LocalTime", localTime));
        }
    }

    public static class SerilogExtension
    {
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";

        private static readonly string[] SensitiveHeaders =
        {
            "Authorization",
            "Cookie",
            "Set-Cookie",
            "Password",
            "RefreshToken",
            "AccessToken"
        };

        private static readonly string[] ExcludedPaths =
        {
            "/swagger",
            "/health",
            "/favicon.ico"
        };

        private static bool ShouldSkipLogging(PathString path)
        {
            return ExcludedPaths.Any(x =>
                path.StartsWithSegments(x,
                    StringComparison.OrdinalIgnoreCase));
        }

        public static void AddSharedSerilog(this WebApplicationBuilder builder)
        {
            //builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection("Logging"));

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.With<LocalTimeEnricher>()
                .Enrich.WithDemystifiedStackTraces()
                .CreateLogger();

            builder.Host.UseSerilog();
        }

        public static void UseSharedSerilogRequestLogging(this WebApplication app, Action<LoggingOptions>? configure = null)
        {
            // Lấy cấu hình đã đăng ký từ DI qua IOptions
            var options = app.Services.GetRequiredService<IOptions<LoggingOptions>>().Value;

            // 1. Middleware tự viết để xử lý CorrelationId, Request/Response Body và Structured Log chi tiết
            app.Use(async (context, next) =>
            {
                if (ShouldSkipLogging(context.Request.Path))
                {
                    await next();
                    return;
                }

                #region Xử lý CorrelationId
                if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
                {
                    correlationId = Guid.NewGuid().ToString();
                }

                // Trả về CorrelationId trong Header của Response để Client tiện tra cứu khi cần
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderKey))
                {
                    context.Response.Headers[CorrelationIdHeaderKey] = correlationId;
                }
                #endregion

                // Đẩy các thuộc tính ngữ cảnh vào Scope Log để các hàm Log.Information/Warning thông thường trong Controller cũng bắt được
                using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
                using (LogContext.PushProperty("TraceId", Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier))
                using (LogContext.PushProperty("SpanId", Activity.Current?.SpanId.ToString()))
                {
                    string requestBody = string.Empty;
                    Dictionary<string, string> requestHeaders = new();

                    // Sao lưu lại luồng Response gốc của hệ thống
                    var originalResponseBody = context.Response.Body;

                    // Khởi tạo một MemoryStream tạm thời để hứng dữ liệu Response xuất ra
                    await using var responseBodyStream = new MemoryStream();
                    context.Response.Body = responseBodyStream;

                    try
                    {
                        #region Xử lý và đọc nội dung Request
                        requestHeaders = context.Request.Headers.ToDictionary(
                            h => h.Key,
                            h => MaskSensitiveValue(h.Key, h.Value.ToString())
                        );

                        if (options.LogRequestBody && context.Request.ContentLength > 0 && context.Request.Body.CanRead && IsLoggableContentType(context.Request.ContentType))
                        {
                            context.Request.EnableBuffering();
                            context.Request.Body.Position = 0;
                            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                            {
                                requestBody = await reader.ReadToEndAsync();
                            }
                            context.Request.Body.Position = 0; // Reset lại đầu Stream cho các service phía sau đọc tiếp
                            requestBody = TrimBody(requestBody, options.MaxBodyLength);
                        }
                        #endregion

                        // Tiếp tục luân chuyển Request đến các Middleware tiếp theo (YARP Gateway, Controller, v.v.)
                        await next();

                        // Xác định trạng thái của Response sau khi các tầng xử lý xong
                        var isSuccess = context.Response.StatusCode < 400;
                        var shouldLog = (isSuccess && options.LogSuccessResponse) || (!isSuccess && options.LogFailedResponse);

                        if (shouldLog)
                        {
                            #region Xử lý và đọc nội dung Response Headers (Luôn an toàn)
                            var responseHeaders = context.Response.Headers.ToDictionary(
                                h => h.Key,
                                h => MaskSensitiveValue(h.Key, h.Value.ToString())
                            );
                            #endregion

                            #region Xử lý và đọc nội dung Response Body (Được bọc bẫy lỗi riêng biệt)
                            string responseBody = string.Empty;
                            if (options.LogResponseBody && IsLoggableContentType(context.Response.ContentType))
                            {
                                try
                                {
                                    if (context.Response.Body.CanSeek)
                                    {
                                        context.Response.Body.Seek(0, SeekOrigin.Begin);
                                        using (var responseReader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true))
                                        {
                                            responseBody = await responseReader.ReadToEndAsync();
                                        }
                                        context.Response.Body.Seek(0, SeekOrigin.Begin); // Trả lại vị trí đầu để copy về luồng gốc
                                        responseBody = TrimBody(responseBody, options.MaxBodyLength);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Ghi nhận log lỗi nội bộ nếu không thể dịch chuyển con trỏ Stream, không làm crash luồng chính
                                    SelfLog.WriteLine($"[Serilog Extension Error] Không thể đọc Response Body: {ex.Message}");
                                }
                            }
                            #endregion

                            #region Thực hiện tạo Structured Logging chi tiết (Headers luôn xuất hiện ở đây)
                            var logger = Log.ForContext("RequestMethod", context.Request.Method)
                                           .ForContext("RequestPath", context.Request.Path.ToString())
                                           .ForContext("QueryString", context.Request.QueryString.ToString())
                                           .ForContext("StatusCode", context.Response.StatusCode)
                                           .ForContext("RequestHeaders", requestHeaders, true)
                                           .ForContext("ResponseHeaders", responseHeaders, true);

                            if (!string.IsNullOrWhiteSpace(requestBody))
                                logger = logger.ForContext("RequestBody", requestBody, destructureObjects: true);

                            if (!string.IsNullOrWhiteSpace(responseBody))
                                logger = logger.ForContext("ResponseBody", responseBody, destructureObjects: true);

                            if (isSuccess)
                                logger.Information("HTTP Request Completed");
                            else
                                logger.Warning("HTTP Request Failed");
                            #endregion
                        }

                        // Sao chép toàn bộ dữ liệu từ Stream tạm thời về Stream gốc trả cho Client nhận dữ liệu
                        responseBodyStream.Seek(0, SeekOrigin.Begin);
                        await responseBodyStream.CopyToAsync(originalResponseBody);
                    }
                    catch (Exception ex)
                    {
                        // Trình bẫy lỗi tối cao phòng trường hợp hệ thống sập hoàn toàn
                        Log.ForContext("RequestMethod", context.Request.Method)
                           .ForContext("RequestPath", context.Request.Path.ToString())
                           .ForContext("RequestHeaders", requestHeaders, true)
                           //.ForContext("ErrorMessage", ex.Message)
                           .Error(ex, "HTTP Request Fatal Error");
                        throw;
                    }
                    finally
                    {
                        // Hoàn trả lại Stream gốc cho Context để tránh rò rỉ bộ nhớ hệ thống
                        context.Response.Body = originalResponseBody;
                    }
                }
            });

            // 2. Kích hoạt tính năng ghi nhận tóm tắt dòng đơn của Serilog, nạp thêm CorrelationId vào Context chuẩn của Serilog
            app.UseSerilogRequestLogging(serilogOptions =>
            {
                serilogOptions.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

                // Kỹ thuật đồng bộ hóa ngữ cảnh (Enrichment)
                serilogOptions.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    if (httpContext.Response.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationIdValue))
                    {
                        diagnosticContext.Set("CorrelationId", correlationIdValue.ToString());
                    }
                };
            });

            // 3. Đăng ký giải phóng bộ nhớ đệm an toàn khi ứng dụng nhận tín hiệu Shutdown tắt máy
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        }

        private static string TrimBody(string? body, int maxLength = 5000)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            return body.Length <= maxLength
                ? body
                : body[..maxLength] + "...(trimmed)";
        }

        private static string MaskSensitiveValue(string key, string value)
        {
            if (SensitiveHeaders.Any(x => key.Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                return "***MASKED***";
            }

            return value;
        }

        private static bool IsLoggableContentType(string? contentType, bool allowHtml = false)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return false;
            }
            if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (contentType.Contains("application/problem+json", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (contentType.Contains("application/xml", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (contentType.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (contentType.Contains("text/plain", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (allowHtml && contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
