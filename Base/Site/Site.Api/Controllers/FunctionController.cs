using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.Features.Function.CreateFunction;
using Site.Application.Features.Function.DeleteFunction;
using Site.Application.Features.Function.ExportFunctions;
using Site.Application.Features.Function.GetAllFunctions;
using Site.Application.Features.Function.GetFunctionById;
using Site.Application.Features.Function.ImportFunctions;
using Site.Application.Features.Function.UpdateFunction;
using Site.Contract.DTOs;

namespace Site.Api.Controllers
{
    [Route("api/function")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class FunctionController : BaseController<FunctionController>
    {
        public FunctionController(ILogger<FunctionController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [RequireFunction("CreateFunction")]
        public async Task<IActionResult> CreateFunction([FromBody] FunctionInputDTO data)
        {
            CreateFunctionCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<FunctionOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{functionId}")]
        [RequireFunction("UpdateFunction")]
        public async Task<IActionResult> UpdateFunction([FromRoute] string functionId, [FromBody] FunctionInputDTO data)
        {
            data.FunctionId = functionId;
            UpdateFunctionCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<FunctionOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{functionId}")]
        public async Task<IActionResult> GetFunction([FromRoute] string functionId)
        {
            GetFunctionByIdQuery query = new GetFunctionByIdQuery()
            {
                Data = functionId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<FunctionOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllFunctions([FromQuery] FunctionFilterDTO filter)
        {
            GetAllFunctionsQuery query = new GetAllFunctionsQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<FunctionOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportFunctions()
        {
            ExportFunctionsQuery query = new();
            var result = await _mediator.Send(query);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{functionId}")]
        [RequireFunction("DeleteFunction")]
        public async Task<IActionResult> DeleteFunction([FromRoute] string functionId, [FromBody] DeleteFunctionInputDTO data)
        {
            data.FunctionId = functionId;
            DeleteFunctionCommand command = new()
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

        [HttpPost("import-functions")]
        [RequireFunction("ImportFunctions")]
        public async Task<IActionResult> ImportFunctions([FromForm] ImportFunctionsInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportFunctionsCommand command = new()
            {
                Data = stream,
                FileName = file.FileName
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<FunctionOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
