using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommonLibs.Comtrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<T> : ControllerBase
    {
        protected readonly ILogger<T> _logger;
        protected readonly IMediator _mediator;
        protected readonly IHttpContextAccessor _httpContext;

        public BaseController(ILogger<T> logger, IMediator mediator, IHttpContextAccessor httpContext)
        {
            _logger = logger;
            _mediator = mediator;
            _httpContext = httpContext;
        }

        /// <summary>
        /// True nếu user hiện tại là Admin toàn hệ thống (Role "Admin"), hoặc là Admin riêng của đúng site
        /// <paramref name="siteId"/> (claim "admin_site" khớp SiteId này, gán từ Login() theo Role "Admin" của
        /// user cho từng Site cụ thể). Dùng cho các action cần cho phép "Admin của site X" thao tác trên site X
        /// mà không cần quyền Admin toàn hệ thống.
        /// </summary>
        protected bool IsSiteAdmin(string siteId)
        {
            var user = _httpContext.HttpContext?.User;
            if (user == null)
            {
                return false;
            }
            if (user.IsInRole("Admin"))
            {
                return true;
            }
            return user.HasClaim("admin_site", siteId);
        }
    }
}
