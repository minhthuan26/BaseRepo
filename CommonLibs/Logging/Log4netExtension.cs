using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using log4net;
using log4net.Config;

namespace CommonLibs.Logging
{
    public static class Log4netExtension
    {
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";

        // Logger gốc dùng để in ra Console (dành cho Dev đọc)
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Log4netExtension));

        // Logger chuyên dụng dùng để ghi file JSON chuẩn ElasticSearch
        private static readonly ILog _elasticLogger = LogManager.GetLogger("ElasticSearchLogger");

        private static readonly string[] SensitiveHeaders =
        {
            "Authorization", "Cookie", "Set-Cookie", "Password", "RefreshToken", "AccessToken"
        };

        private static readonly string[] ExcludedPaths =
        {
            "/swagger", "/health", "/favicon.ico"
        };

        private static bool ShouldSkipLogging(PathString path)
        {
            return ExcludedPaths.Any(x =>
                path.StartsWithSegments(x, StringComparison.OrdinalIgnoreCase));
        }

        public static void AddSharedLog4net(this WebApplicationBuilder builder)
        {
            // (Lưu ý: Nếu bạn đã gọi Configure<LoggingOptions> ở DynamicLoggingExtension thì có thể comment dòng dưới này)
            // builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection("Logging"));

            // 1. Dọn dẹp các Provider log mặc định của .NET (Console gốc)
            builder.Logging.ClearProviders();

            // 2. Cấp đường dẫn gốc của project cho Log4net (Phục vụ cấu hình ghi File)
            log4net.GlobalContext.Properties["LogRootPath"] = builder.Environment.ContentRootPath;

            // 3. Tích hợp log4net vào ILogger của .NET
            builder.Logging.AddLog4Net("log4net.config");

            // 4. Khởi tạo cấu hình cho các đối tượng tĩnh của log4net
            //var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            //XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        public static void UseSharedLog4netRequestLogging(this WebApplication app, Action<LoggingOptions>? configure = null)
        {
            var options = app.Services.GetRequiredService<IOptions<LoggingOptions>>().Value;

            app.Use(async (context, next) =>
            {
                if (ShouldSkipLogging(context.Request.Path))
                {
                    await next();
                    return;
                }

                var stopwatch = Stopwatch.StartNew();

                #region Xử lý CorrelationId
                if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
                {
                    correlationId = Guid.NewGuid().ToString();
                }

                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderKey))
                {
                    context.Response.Headers[CorrelationIdHeaderKey] = correlationId;
                }
                #endregion

                LogicalThreadContext.Properties["CorrelationId"] = correlationId.ToString();
                LogicalThreadContext.Properties["TraceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
                LogicalThreadContext.Properties["SpanId"] = Activity.Current?.SpanId.ToString();

                string requestBody = string.Empty;
                Dictionary<string, string> requestHeaders = new();

                var originalResponseBody = context.Response.Body;
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
                        context.Request.Body.Position = 0;
                        requestBody = TrimBody(requestBody, options.MaxBodyLength);
                    }
                    #endregion

                    await next();

                    stopwatch.Stop();
                    var isSuccess = context.Response.StatusCode < 400;
                    var shouldLog = (isSuccess && options.LogSuccessResponse) || (!isSuccess && options.LogFailedResponse);

                    if (shouldLog)
                    {
                        #region Xử lý và đọc nội dung Response Headers
                        var responseHeaders = context.Response.Headers.ToDictionary(
                            h => h.Key,
                            h => MaskSensitiveValue(h.Key, h.Value.ToString())
                        );
                        #endregion

                        #region Xử lý và đọc nội dung Response Body
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
                                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                                    responseBody = TrimBody(responseBody, options.MaxBodyLength);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Warn($"[Log4net Extension Error] Không thể đọc Response Body: {ex.Message}");
                            }
                        }
                        #endregion

                        #region Thực hiện Logging JSON chuẩn Serilog
                        var serilogLevel = isSuccess ? "Information" : "Warning";
                        var serilogMessage = isSuccess ? "HTTP Request Completed" : "HTTP Request Failed";

                        // 1. Tạo đối tượng lồng nhau giống hệt ExpressionTemplate của Serilog
                        var logPayload = new Dictionary<string, object>
                        {
                            ["@t"] = DateTime.UtcNow.ToString("O"),
                            ["LocalTime"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                            ["@l"] = serilogLevel,
                            ["Application"] = "Gateway-Service",
                            ["CorrelationId"] = correlationId.ToString(),
                            ["Message"] = serilogMessage,
                            ["Properties"] = new
                            {
                                ResponseBody = string.IsNullOrWhiteSpace(responseBody) ? null : responseBody,
                                ResponseHeaders = responseHeaders,
                                RequestHeaders = requestHeaders,
                                StatusCode = context.Response.StatusCode,
                                QueryString = context.Request.QueryString.ToString(),
                                RequestPath = context.Request.Path.ToString(),
                                RequestMethod = context.Request.Method,
                                SpanId = Activity.Current?.SpanId.ToString(),
                                TraceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier,
                                RequestId = context.TraceIdentifier,
                                ConnectionId = context.Connection.Id,
                                RequestBody = string.IsNullOrWhiteSpace(requestBody) ? null : requestBody
                            }
                        };

                        // 2. Chuyển thành JSON (Bỏ qua các trường null để tối ưu dung lượng)
                        var jsonOptions = new JsonSerializerOptions
                        {
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        };
                        var jsonMessage = JsonSerializer.Serialize(logPayload, jsonOptions);

                        // 3. In Log rút gọn ra Console
                        var summaryMessage = $"HTTP {context.Request.Method} {context.Request.Path} responded {context.Response.StatusCode} in {stopwatch.Elapsed.TotalMilliseconds:0.0000}ms";
                        if (isSuccess)
                            _logger.Info(summaryMessage);
                        else
                            _logger.Warn(summaryMessage);

                        // 4. Đẩy chuỗi JSON nguyên vẹn vào File cho ElasticSearch
                        if (isSuccess)
                            _elasticLogger.Info(jsonMessage);
                        else
                            _elasticLogger.Warn(jsonMessage);
                        #endregion
                    }

                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(originalResponseBody);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.Fatal($"HTTP Request Fatal Error | {context.Request.Method} {context.Request.Path}", ex);
                    throw;
                }
                finally
                {
                    context.Response.Body = originalResponseBody;

                    // Dọn dẹp biến LogicalThreadContext để tránh rò rỉ bộ nhớ
                    LogicalThreadContext.Properties.Remove("CorrelationId");
                    LogicalThreadContext.Properties.Remove("TraceId");
                    LogicalThreadContext.Properties.Remove("SpanId");
                }
            });

            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopped.Register(() => LogManager.Shutdown());
        }

        private static string TrimBody(string? body, int maxLength = 5000)
        {
            if (string.IsNullOrWhiteSpace(body)) return string.Empty;
            return body.Length <= maxLength ? body : body[..maxLength] + "...(trimmed)";
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
            if (string.IsNullOrWhiteSpace(contentType)) return false;

            if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("application/problem+json", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("application/xml", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("text/plain", StringComparison.OrdinalIgnoreCase))
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