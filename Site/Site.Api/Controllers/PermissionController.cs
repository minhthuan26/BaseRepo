using CommonLibs.Comtrollers;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.Features.Permission.AssignRolePermissions;
using Site.Application.Features.Permission.GetPermissionTree;
using Site.Contract.DTOs;

namespace Site.Api.Controllers
{
    [Route("api/permission")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class PermissionController : BaseController<PermissionController>
    {
        public PermissionController(ILogger<PermissionController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpGet("{siteId}")]
        public async Task<IActionResult> GetPermissionTree([FromRoute] string siteId, [FromQuery] string? roleId)
        {
            // Bỏ qua siteId client truyền trên route - luôn dùng đúng Site của người gọi (claim "site_id"), tránh
            // để bất kỳ user đã đăng nhập nào (route này không có [RequireFunction]) xem được cây quyền/Role-Function
            // của Site khác chỉ bằng cách đổi siteId trên URL.
            GetPermissionTreeQuery query = new GetPermissionTreeQuery()
            {
                SiteId = CurrentSiteId,
                RoleId = roleId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<PermissionTreeDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("role/{roleId}")]
        [RequireFunction("AssignRolePermissions")]
        public async Task<IActionResult> AssignRolePermissions([FromRoute] string roleId, [FromBody] AssignRolePermissionsInputDTO data)
        {
            AssignRolePermissionsCommand command = new()
            {
                RoleId = roleId,
                FunctionIds = data.FunctionIds,
                // Chỉ được cấp quyền cho Role thuộc đúng Site của người gọi.
                SiteId = CurrentSiteId
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<string>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
