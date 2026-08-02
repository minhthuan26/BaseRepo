using CommonLibs.Utils;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CommonLibs.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        var correlationId =
            context.Response.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? string.Empty;

        string requestBody = string.Empty;

        try
        {
            if (context.Items.TryGetValue("RequestBody", out var body))
            {
                requestBody = body?.ToString() ?? string.Empty;
            }
            else if (context.Request.Body.CanSeek)
            {
                context.Request.Body.Position = 0;

                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    leaveOpen: true);

                requestBody = await reader.ReadToEndAsync();

                context.Request.Body.Position = 0;
            }
        }
        catch
        {
            requestBody = "[Unable to read request body]";
        }

        var requestHeaders = context.Request.Headers.ToDictionary(
            h => h.Key,
            h => h.Value.ToString());

        context.Response.Clear();
        context.Response.ContentType = "application/json; charset=utf-8";

        // AppException là lỗi nghiệp vụ đã được service tự throw kèm đúng HttpStatusCode + message dành cho
        // client (VD 400 sai mật khẩu, 404 không tìm thấy...) - phải trả nguyên Code/Message đó thay vì luôn
        // ép về 500, nếu không FE không cách nào phân biệt được lỗi nghiệp vụ với lỗi hệ thống thật.
        var isBusinessException = exception is AppException;
        var statusCode = isBusinessException ? ((AppException)exception).Code : HttpStatusCode.InternalServerError;
        context.Response.StatusCode = (int)statusCode;

        var logContext = Log.ForContext("CorrelationId", correlationId)
           .ForContext("TraceId", traceId)
           .ForContext("RequestMethod", context.Request.Method)
           .ForContext("RequestPath", context.Request.Path.ToString())
           .ForContext("QueryString", context.Request.QueryString.ToString())
           .ForContext("RequestHeaders", requestHeaders, true)
           .ForContext("RequestBody", requestBody, destructureObjects: true)
           .ForContext("StatusCode", context.Response.StatusCode);

        var response = new Response<object>
        {
            Success = false,
            Code = statusCode,
            Message = isBusinessException
                ? exception.Message
                : $"Hệ thống có sự cố. Vui lòng liên hệ Admin và cung cấp TraceId: {traceId}",
            Data = null
        };

        if (isBusinessException)
        {
            logContext.Warning(exception, "HTTP Request Business Exception");
        }
        else
        {
            logContext.Error(exception, "HTTP Request Fatal Error");
        }

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));
    }
}