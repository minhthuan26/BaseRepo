using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Utils
{
    public interface IAuditContext
    {
        string? UserName { get; }
        string? DeviceId { get; }
        string? DeviceName { get; }
        string? CorrelationId { get; }
    }
    public class AuditContext : IAuditContext
    {
        private readonly IHttpContextAccessor _httpContext;

        public AuditContext(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        public string? UserName =>
            _httpContext.HttpContext?.Items["UserName"]?.ToString();

        public string? DeviceId =>
            _httpContext.HttpContext?.Items["DeviceId"]?.ToString();

        public string? DeviceName =>
            _httpContext.HttpContext?.Items["DeviceName"]?.ToString();

        public string? CorrelationId =>
            _httpContext.HttpContext?.Items["CorrelationId"]?.ToString();
    }
}
