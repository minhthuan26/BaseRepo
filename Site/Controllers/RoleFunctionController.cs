using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.Features.RoleFunction.CreateRoleFunction;
using Site.Application.Features.RoleFunction.DeleteRoleFunction;
using Site.Application.Features.RoleFunction.ExportRoleFunctions;
using Site.Application.Features.RoleFunction.GetAllRoleFunctions;
using Site.Application.Features.RoleFunction.GetFunctionsByRole;
using Site.Application.Features.RoleFunction.GetRoleFunctionById;
using Site.Application.Features.RoleFunction.ImportRoleFunctions;
using Site.Domain.DTOs;

namespace Site.Controllers
{
    [Route("api/role-function")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class RoleFunctionController : BaseController<RoleFunctionController>
    {
        public RoleFunctionController(ILogger<RoleFunctionController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [RequireFunction("CreateRoleFunction")]
        public async Task<IActionResult> CreateRoleFunction([FromBody] RoleFunctionInputDTO data)
        {
            CreateRoleFunctionCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<RoleFunctionOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{roleId}/{functionId}")]
        public async Task<IActionResult> GetRoleFunction([FromRoute] string roleId, [FromRoute] string functionId)
        {
            GetRoleFunctionByIdQuery query = new GetRoleFunctionByIdQuery()
            {
                RoleId = roleId,
                FunctionId = functionId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<RoleFunctionOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetFunctionsByRole([FromRoute] string roleId)
        {
            GetFunctionsByRoleQuery query = new GetFunctionsByRoleQuery()
            {
                RoleId = roleId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<List<FunctionOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllRoleFunctions([FromQuery] RoleFunctionFilterDTO filter)
        {
            GetAllRoleFunctionsQuery query = new GetAllRoleFunctionsQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<RoleFunctionOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportRoleFunctions()
        {
            ExportRoleFunctionsQuery query = new();
            var result = await _mediator.Send(query);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{roleId}/{functionId}")]
        [RequireFunction("DeleteRoleFunction")]
        public async Task<IActionResult> DeleteRoleFunction([FromRoute] string roleId, [FromRoute] string functionId, [FromBody] DeleteRoleFunctionInputDTO data)
        {
            data.RoleId = roleId;
            data.FunctionId = functionId;
            DeleteRoleFunctionCommand command = new()
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

        [HttpPost("import-role-functions")]
        [RequireFunction("ImportRoleFunctions")]
        public async Task<IActionResult> ImportRoleFunctions([FromForm] ImportRoleFunctionsInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportRoleFunctionsCommand command = new()
            {
                Data = stream,
                FileName = file.FileName
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<RoleFunctionOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
