using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.Features.Role.CreateRole;
using Site.Application.Features.Role.DeleteRole;
using Site.Application.Features.Role.ExportRoles;
using Site.Application.Features.Role.GetAllRoles;
using Site.Application.Features.Role.GetRoleById;
using Site.Application.Features.Role.ImportRoles;
using Site.Application.Features.Role.UpdateRole;
using Site.Contract.DTOs;

namespace Site.Api.Controllers
{
    [Route("api/role")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class RoleController : BaseController<RoleController>
    {
        public RoleController(ILogger<RoleController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [RequireFunction("CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody] RoleInputDTO data)
        {
            // Role mới luôn thuộc đúng Site của người gọi (claim "site_id") - không cho client tự truyền SiteId
            // khác trong body để tạo Role ở Site khác (đặc biệt quan trọng từ khi RoleId được phép trùng giữa
            // các Site - xem CommonLibs/BaseSQL/multi-tenant/08_fix_roleid_unique_scope.sql).
            data.SiteId = CurrentSiteId;
            CreateRoleCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<RoleOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{roleId}")]
        [RequireFunction("UpdateRole")]
        public async Task<IActionResult> UpdateRole([FromRoute] string roleId, [FromBody] RoleInputDTO data)
        {
            data.RoleId = roleId;
            // Chỉ được sửa Role thuộc đúng Site của người gọi - RoleId không còn unique toàn cục nên phải so
            // khớp thêm SiteId ở tầng Repository để không sửa nhầm Role trùng RoleId ở Site khác.
            data.SiteId = CurrentSiteId;
            UpdateRoleCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<RoleOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRole([FromRoute] string roleId)
        {
            GetRoleByIdQuery query = new GetRoleByIdQuery()
            {
                Data = roleId,
                SiteId = CurrentSiteId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<RoleOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllRoles([FromQuery] RoleFilterDTO filter)
        {
            filter.SiteId = CurrentSiteId;
            GetAllRolesQuery query = new GetAllRolesQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<RoleOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportRoles()
        {
            ExportRolesQuery query = new()
            {
                SiteId = CurrentSiteId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{roleId}")]
        [RequireFunction("DeleteRole")]
        public async Task<IActionResult> DeleteRole([FromRoute] string roleId, [FromBody] DeleteRoleInputDTO data)
        {
            data.RoleId = roleId;
            data.SiteId = CurrentSiteId;
            DeleteRoleCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPost("import-roles")]
        [RequireFunction("ImportRoles")]
        public async Task<IActionResult> ImportRoles([FromForm] ImportRolesInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportRolesCommand command = new()
            {
                Data = stream,
                FileName = file.FileName,
                SiteId = CurrentSiteId
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<RoleOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
