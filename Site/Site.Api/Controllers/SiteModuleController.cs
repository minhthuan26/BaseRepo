using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.Features.SiteModule.GetSiteModulesBySite;
using Site.Application.Features.SiteModule.ProvisionSiteModule;
using Site.Application.Features.SiteModule.ToggleSiteModule;
using Site.Contract.DTOs;

namespace Site.Api.Controllers
{
    /// <summary>
    /// Quản lý entitlement + bật/tắt Module theo Site (tbl_Site_Module). Xem tài liệu thiết kế multi-tenant
    /// RBAC, mục 04+07, và README của CommonLibs/BaseSQL/multi-tenant/.
    ///
    /// QUAN TRỌNG: 2 action Provision/GetSiteModulesBySite bên dưới KHÔNG được thêm route YARP trong
    /// Gateway/appsettings.json - chỉ gọi được trực tiếp vào cổng nội bộ của Site service (không đi qua
    /// Gateway công khai), dành cho tool/đội vận hành. Action GetMySiteModules/ToggleSiteModule thì có
    /// route YARP bình thường - Site Admin gọi qua Gateway như mọi endpoint khác.
    /// </summary>
    [Route("api/site-module")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class SiteModuleController : BaseController<SiteModuleController>
    {
        public SiteModuleController(ILogger<SiteModuleController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        // ===================== NỘI BỘ - KHÔNG CÓ ROUTE YARP (xem Gateway/appsettings.json) =====================

        /// <summary>Đội vận hành cấp Module mới cho 1 Site (Site vừa mua thêm gói). Mặc định tạo dòng ở
        /// trạng thái TẮT - Site Admin tự bật lại sau qua ToggleSiteModule.</summary>
        [HttpPost("provision")]
        [RequireFunction("ProvisionSiteModule")]
        public async Task<IActionResult> ProvisionSiteModule([FromBody] ProvisionSiteModuleInputDTO data)
        {
            ProvisionSiteModuleCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<SiteModuleOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        /// <summary>Đội vận hành xem toàn bộ entitlement (mọi trạng thái) của 1 Site bất kỳ để đối soát -
        /// khác GetMySiteModules ở chỗ SiteId truyền tự do qua route, không giới hạn theo Site của người gọi.</summary>
        [HttpGet("site/{siteId}")]
        public async Task<IActionResult> GetSiteModulesBySite([FromRoute] string siteId)
        {
            GetSiteModulesBySiteQuery query = new()
            {
                SiteId = siteId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<List<SiteModuleOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        // ===================== CÔNG KHAI QUA GATEWAY - Site Admin tự quản lý Module của Site mình =====================

        /// <summary>Site Admin xem danh sách Module đã được cấp (mọi trạng thái bật/tắt) cho ĐÚNG Site của
        /// chính mình - SiteId luôn lấy từ claim "site_id" trong token, không nhận qua tham số.</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMySiteModules()
        {
            if (string.IsNullOrWhiteSpace(CurrentSiteId))
            {
                throw new AppException("SiteId is missing in token claims.", System.Net.HttpStatusCode.BadRequest);
            }

            GetSiteModulesBySiteQuery query = new()
            {
                SiteId = CurrentSiteId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<List<SiteModuleOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        /// <summary>Site Admin bật/tắt 1 Module đã được cấp cho Site của chính mình. Trả 404 nếu Module chưa
        /// được đội vận hành cấp (chưa mua) - không tự tạo dòng mới ở đây.</summary>
        [HttpPut("toggle")]
        [RequireFunction("ToggleSiteModule")]
        public async Task<IActionResult> ToggleSiteModule([FromBody] ToggleSiteModuleInputDTO data)
        {
            if (string.IsNullOrWhiteSpace(CurrentSiteId))
            {
                throw new AppException("SiteId is missing in token claims.", System.Net.HttpStatusCode.BadRequest);
            }

            ToggleSiteModuleCommand command = new()
            {
                SiteId = CurrentSiteId,
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<SiteModuleOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
