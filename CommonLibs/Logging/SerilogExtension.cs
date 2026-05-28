using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using System.Text;

namespace CommonLibs.Logging
{
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

        public static void AddSharedSerilog(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithDemystifiedStackTraces()
                .CreateLogger();

            builder.Host.UseSerilog();
        }

        public static void UseSharedSerilogRequestLogging(this WebApplication app)
        {
            app.Use(async (context, next) =>
            {
                #region CorrelationId

                if (!context.Request.Headers.TryGetValue(
                    CorrelationIdHeaderKey,
                    out var correlationId))
                {
                    correlationId = Guid.NewGuid().ToString();
                }

                context.Response.Headers[CorrelationIdHeaderKey] =
                    correlationId;

                #endregion

                using (LogContext.PushProperty(
                    "CorrelationId",
                    correlationId.ToString()))
                {
                    #region Request

                    context.Request.EnableBuffering();

                    string requestBody = string.Empty;

                    if (context.Request.ContentLength > 0
                        && context.Request.Body.CanRead
                        && IsLoggableContentType(context.Request.ContentType))
                    {
                        context.Request.Body.Position = 0;

                        using var reader = new StreamReader(
                            context.Request.Body,
                            Encoding.UTF8,
                            leaveOpen: true);

                        requestBody = await reader.ReadToEndAsync();

                        context.Request.Body.Position = 0;

                        requestBody = TrimBody(requestBody);
                    }

                    var requestHeaders = context.Request.Headers
                        .ToDictionary(
                            h => h.Key,
                            h => MaskSensitiveValue(
                                h.Key,
                                h.Value.ToString()));

                    #endregion

                    #region Response

                    var originalResponseBody = context.Response.Body;

                    await using var responseBodyMemoryStream =
                        new MemoryStream();

                    context.Response.Body = responseBodyMemoryStream;

                    #endregion

                    try
                    {
                        await next();

                        #region Read Response Body

                        context.Response.Body.Seek(0, SeekOrigin.Begin);

                        string responseBody = string.Empty;

                        if (IsLoggableContentType(context.Response.ContentType))
                        {
                            responseBody =
                                await new StreamReader(context.Response.Body)
                                    .ReadToEndAsync();

                            responseBody = TrimBody(responseBody);
                        }

                        context.Response.Body.Seek(0, SeekOrigin.Begin);

                        #endregion

                        #region Response Headers

                        var responseHeaders = context.Response.Headers
                            .ToDictionary(
                                h => h.Key,
                                h => MaskSensitiveValue(
                                    h.Key,
                                    h.Value.ToString()));

                        #endregion

                        #region Logging

                        Log.Information(
                            @"
                            HTTP Request
                            CorrelationId: {CorrelationId}
                            RequestMethod: {RequestMethod}
                            RequestPath: {RequestPath}
                            QueryString: {QueryString}
                            StatusCode: {StatusCode}
                            RequestHeaders: {@RequestHeaders}
                            RequestBody: {RequestBody}
                            ResponseHeaders: {@ResponseHeaders}
                            ResponseBody: {ResponseBody}
                            ",
                            correlationId.ToString(),
                            context.Request.Method,
                            context.Request.Path,
                            context.Request.QueryString.ToString(),
                            context.Response.StatusCode,
                            requestHeaders,
                            requestBody,
                            responseHeaders,
                            responseBody);

                        #endregion

                        #region Copy Response

                        await responseBodyMemoryStream.CopyToAsync(
                            originalResponseBody);

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Log.Error(
                            ex,
                            @"
                            HTTP Request Error
                            CorrelationId: {CorrelationId}
                            RequestMethod: {RequestMethod}
                            RequestPath: {RequestPath}
                            ",
                            correlationId.ToString(),
                            context.Request.Method,
                            context.Request.Path);

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
                options.MessageTemplate =
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
            });

            var lifetime =
                app.Services.GetRequiredService<IHostApplicationLifetime>();

            lifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        }

        private static string TrimBody(
            string? body,
            int maxLength = 5000)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            return body.Length <= maxLength
                ? body
                : body[..maxLength] + "...(trimmed)";
        }

        private static string MaskSensitiveValue(
            string key,
            string value)
        {
            if (SensitiveHeaders.Any(x =>
                    key.Contains(
                        x,
                        StringComparison.OrdinalIgnoreCase)))
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
