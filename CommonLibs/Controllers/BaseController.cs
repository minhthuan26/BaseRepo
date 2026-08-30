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
        /// True nếu user hiện tại là Admin riêng của đúng site <paramref name="siteId"/> (claim "admin_site"
        /// khớp SiteId này, gán từ Login() theo Role tên "Admin" mà user đang giữ CHO ĐÚNG SITE ĐÓ). Dùng cho
        /// các action cần cho phép "Admin của site X" thao tác trên site X.
        ///
        /// KHÔNG còn nhánh bypass "Admin toàn hệ thống" (đã bỏ chủ ý) - trước đây user.IsInRole("Admin") tự
        /// động trả true bất kể siteId truyền vào là gì, nghĩa là Admin của Site A gọi được IsSiteAdmin("SiteB")
        /// và thao tác được lên dữ liệu Site B. Vì mỗi Site giờ tự có Role "Admin" riêng (multi-tenant), so khớp
        /// literal RoleName không còn phân biệt được "Admin của Site nào" - chỉ còn cách tin theo đúng claim
        /// "admin_site" (đã được Login() gán chính xác theo SiteId của user).
        /// </summary>
        protected bool IsSiteAdmin(string siteId)
        {
            var user = _httpContext.HttpContext?.User;
            if (user == null)
            {
                return false;
            }
            return user.HasClaim("admin_site", siteId);
        }

        /// <summary>
        /// SiteId của user hiện tại (claim "site_id", gán từ Login() theo tài liệu thiết kế multi-tenant RBAC -
        /// mỗi User chỉ thuộc đúng 1 Site). Dùng cho các action Site Admin tự thao tác trên đúng Site của mình
        /// (VD SiteModuleController.ToggleSiteModule) mà KHÔNG tin theo SiteId client tự truyền lên, để tránh
        /// Site Admin của Site A vô tình/cố ý thao tác lên dữ liệu Site B.
        /// </summary>
        protected string? CurrentSiteId => _httpContext.HttpContext?.User?.FindFirst("site_id")?.Value;
    }
}
