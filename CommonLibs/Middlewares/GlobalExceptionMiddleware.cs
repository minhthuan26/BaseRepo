using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Trích xuất các thông tin ngữ cảnh quan trọng của Request gây lỗi
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        // Log lỗi một cách rõ ràng, mạch lạc
        _logger.LogError(exception, "CRITICAL ERROR: [Method: {Method}] [Path: {Path}] -> Message: {Message}",
            requestMethod, requestPath, exception.Message);

        // Trả về phản hồi sạch sẽ, bảo mật cho Client (không lộ Stack Trace ra ngoài API)
        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Hệ thống có sự cố. Vui lòng liên hệ Admin kèm mã Trace ID phía trên.",
            Detailed = exception.Message // Có thể ẩn đi ở môi trường Production
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}