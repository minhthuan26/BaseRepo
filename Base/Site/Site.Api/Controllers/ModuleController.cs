using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.Features.Module.CreateModule;
using Site.Application.Features.Module.DeleteModule;
using Site.Application.Features.Module.ExportModules;
using Site.Application.Features.Module.GetAllModules;
using Site.Application.Features.Module.GetModuleById;
using Site.Application.Features.Module.ImportModules;
using Site.Application.Features.Module.UpdateModule;
using Site.Contract.DTOs;

namespace Site.Api.Controllers
{
    [Route("api/module")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class ModuleController : BaseController<ModuleController>
    {
        public ModuleController(ILogger<ModuleController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [RequireFunction("CreateModule")]
        public async Task<IActionResult> CreateModule([FromBody] ModuleInputDTO data)
        {
            CreateModuleCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<ModuleOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{moduleId}")]
        [RequireFunction("UpdateModule")]
        public async Task<IActionResult> UpdateModule([FromRoute] string moduleId, [FromBody] ModuleInputDTO data)
        {
            data.ModuleId = moduleId;
            UpdateModuleCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<ModuleOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModule([FromRoute] string moduleId)
        {
            GetModuleByIdQuery query = new GetModuleByIdQuery()
            {
                Data = moduleId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<ModuleOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllModules([FromQuery] ModuleFilterDTO filter)
        {
            GetAllModulesQuery query = new GetAllModulesQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<ModuleOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportModules()
        {
            ExportModulesQuery query = new();
            var result = await _mediator.Send(query);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{moduleId}")]
        [RequireFunction("DeleteModule")]
        public async Task<IActionResult> DeleteModule([FromRoute] string moduleId, [FromBody] DeleteModuleInputDTO data)
        {
            data.ModuleId = moduleId;
            DeleteModuleCommand command = new()
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

        [HttpPost("import-modules")]
        [RequireFunction("ImportModules")]
        public async Task<IActionResult> ImportModules([FromForm] ImportModulesInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportModulesCommand command = new()
            {
                Data = stream,
                FileName = file.FileName
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<ModuleOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
