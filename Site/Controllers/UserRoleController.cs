using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.Features.UserRole.CreateUserRole;
using Site.Application.Features.UserRole.DeleteUserRole;
using Site.Application.Features.UserRole.ExportUserRoles;
using Site.Application.Features.UserRole.GetAllUserRoles;
using Site.Application.Features.UserRole.GetRolesByUser;
using Site.Application.Features.UserRole.GetUserRoleById;
using Site.Application.Features.UserRole.ImportUserRoles;
using Site.Domain.DTOs;

namespace Site.Controllers
{
    [Route("api/user-role")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class UserRoleController : BaseController<UserRoleController>
    {
        public UserRoleController(ILogger<UserRoleController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [RequireFunction("CreateUserRole")]
        public async Task<IActionResult> CreateUserRole([FromBody] UserRoleInputDTO data)
        {
            CreateUserRoleCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<UserRoleOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{userId}/{roleId}")]
        public async Task<IActionResult> GetUserRole([FromRoute] string userId, [FromRoute] string roleId)
        {
            GetUserRoleByIdQuery query = new GetUserRoleByIdQuery()
            {
                UserId = userId,
                RoleId = roleId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<UserRoleOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetRolesByUser([FromRoute] string userId)
        {
            GetRolesByUserQuery query = new GetRolesByUserQuery()
            {
                UserId = userId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<List<RoleOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllUserRoles([FromQuery] UserRoleFilterDTO filter)
        {
            GetAllUserRolesQuery query = new GetAllUserRolesQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<UserRoleOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportUserRoles()
        {
            ExportUserRolesQuery query = new();
            var result = await _mediator.Send(query);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{userId}/{roleId}")]
        [RequireFunction("DeleteUserRole")]
        public async Task<IActionResult> DeleteUserRole([FromRoute] string userId, [FromRoute] string roleId, [FromBody] DeleteUserRoleInputDTO data)
        {
            data.UserId = userId;
            data.RoleId = roleId;
            DeleteUserRoleCommand command = new()
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

        [HttpPost("import-user-roles")]
        [RequireFunction("ImportUserRoles")]
        public async Task<IActionResult> ImportUserRoles([FromForm] ImportUserRolesInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportUserRolesCommand command = new()
            {
                Data = stream,
                FileName = file.FileName
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<UserRoleOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
