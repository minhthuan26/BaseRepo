using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Text;

namespace CommonLibs.Logging
{
    public sealed class LoggingOptions
    {
        public bool LogRequestBody { get; set; } = true;

        public bool LogResponseBody { get; set; } = true;

        public bool LogSuccessResponse { get; set; } = false;

        public bool LogFailedResponse { get; set; } = true;

        public int MaxBodyLength { get; set; } = 5000;
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
            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(builder.Configuration)
               .Enrich.FromLogContext()
               .Enrich.WithDemystifiedStackTraces()
               .CreateLogger();

            builder.Host.UseSerilog();
        }

        public static void UseSharedSerilogRequestLogging(this WebApplication app, Action<LoggingOptions>? configure = null)
        {
            var options = new LoggingOptions();
            configure?.Invoke(options);

            app.Use(async (context, next) =>
            {
                if (ShouldSkipLogging(context.Request.Path))
                {
                    await next();
                    return;
                }

                #region CorrelationId

                if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
                {
                    correlationId = Guid.NewGuid().ToString();
                }

                context.Response.Headers[CorrelationIdHeaderKey] = correlationId;

                #endregion

                using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
                using (LogContext.PushProperty("TraceId", Activity.Current?.TraceId.ToString()))
                using (LogContext.PushProperty("SpanId", Activity.Current?.SpanId.ToString()))
                {
                    string requestBody = string.Empty;
                    Dictionary<string, string> requestHeaders = new();
                    var originalResponseBody = context.Response.Body;
                    await using var responseBodyStream = new MemoryStream();
                    context.Response.Body = responseBodyStream;
                    try
                    {
                        #region Request

                        requestHeaders =
                            context.Request.Headers
                                .ToDictionary(
                                    h => h.Key,
                                    h => MaskSensitiveValue(h.Key, h.Value.ToString())
                                );

                        if (options.LogRequestBody && context.Request.ContentLength > 0 && context.Request.Body.CanRead && IsLoggableContentType(context.Request.ContentType))
                        {
                            context.Request.EnableBuffering();
                            context.Request.Body.Position = 0;
                            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                            requestBody = await reader.ReadToEndAsync();
                            context.Request.Body.Position = 0;
                            requestBody = TrimBody(requestBody, options.MaxBodyLength);
                        }

                        #endregion

                        await next();
                        var isSuccess = context.Response.StatusCode < 400;
                        var shouldLog = (isSuccess && options.LogSuccessResponse) || (!isSuccess && options.LogFailedResponse);
                        string responseBody = string.Empty;
                        Dictionary<string, string> responseHeaders = new();

                        if (shouldLog)
                        {
                            #region Response Headers

                            responseHeaders = context.Response.Headers .ToDictionary(
                                            h => h.Key,
                                            h => MaskSensitiveValue(h.Key, h.Value.ToString())
                                        );

                            #endregion

                            #region Response Body

                            if (options.LogResponseBody)
                            {
                                context.Response.Body.Seek(0, SeekOrigin.Begin);

                                if (IsLoggableContentType(context.Response.ContentType))
                                {
                                    using var responseReader = new StreamReader(context.Response.Body, leaveOpen: true);
                                    responseBody = await responseReader.ReadToEndAsync();
                                    responseBody = TrimBody(responseBody, options.MaxBodyLength);
                                }

                                context.Response.Body.Seek(0, SeekOrigin.Begin);
                            }

                            #endregion

                            #region Structured Logging

                            var logger =
                                Log.ForContext("RequestMethod", context.Request.Method)
                                   .ForContext("RequestPath", context.Request.Path.ToString())
                                   .ForContext("QueryString", context.Request.QueryString.ToString())
                                   .ForContext("StatusCode", context.Response.StatusCode)
                                   .ForContext("RequestHeaders", requestHeaders, true)
                                   .ForContext("ResponseHeaders", responseHeaders, true);

                            if (!string.IsNullOrWhiteSpace(requestBody))
                            {
                                logger = logger.ForContext("RequestBody", requestBody);
                            }

                            if (!string.IsNullOrWhiteSpace(responseBody))
                            {
                                logger = logger.ForContext("ResponseBody", responseBody);
                            }

                            if (isSuccess)
                            {
                                logger.Information("HTTP Request Completed");
                            }
                            else
                            {
                                logger.Warning("HTTP Request Failed");
                            }

                            #endregion
                        }
                        responseBodyStream.Seek(0, SeekOrigin.Begin);
                        await responseBodyStream.CopyToAsync(originalResponseBody);
                    }
                    catch (Exception ex)
                    {
                        Log.ForContext("RequestMethod", context.Request.Method)
                            .ForContext( "RequestPath", context.Request.Path.ToString())
                            .Error(ex, "HTTP Request Error");

                        throw;
                    }
                    finally
                    {
                        context.Response.Body = originalResponseBody;
                    }
                }
            });

            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
            });
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
