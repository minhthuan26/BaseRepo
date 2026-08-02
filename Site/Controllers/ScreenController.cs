using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.Features.Screen.CreateScreen;
using Site.Application.Features.Screen.DeleteScreen;
using Site.Application.Features.Screen.ExportScreens;
using Site.Application.Features.Screen.GetAllScreens;
using Site.Application.Features.Screen.GetScreenById;
using Site.Application.Features.Screen.ImportScreens;
using Site.Application.Features.Screen.UpdateScreen;
using Site.Domain.DTOs;

namespace Site.Controllers
{
    [Route("api/screen")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class ScreenController : BaseController<ScreenController>
    {
        public ScreenController(ILogger<ScreenController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [RequireFunction("CreateScreen")]
        public async Task<IActionResult> CreateScreen([FromBody] ScreenInputDTO data)
        {
            CreateScreenCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<ScreenOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{screenId}")]
        [RequireFunction("UpdateScreen")]
        public async Task<IActionResult> UpdateScreen([FromRoute] string screenId, [FromBody] ScreenInputDTO data)
        {
            data.ScreenId = screenId;
            UpdateScreenCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<ScreenOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{screenId}")]
        public async Task<IActionResult> GetScreen([FromRoute] string screenId)
        {
            GetScreenByIdQuery query = new GetScreenByIdQuery()
            {
                Data = screenId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<ScreenOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllScreens([FromQuery] ScreenFilterDTO filter)
        {
            GetAllScreensQuery query = new GetAllScreensQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<ScreenOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportScreens()
        {
            ExportScreensQuery query = new();
            var result = await _mediator.Send(query);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{screenId}")]
        [RequireFunction("DeleteScreen")]
        public async Task<IActionResult> DeleteScreen([FromRoute] string screenId, [FromBody] DeleteScreenInputDTO data)
        {
            data.ScreenId = screenId;
            DeleteScreenCommand command = new()
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

        [HttpPost("import-screens")]
        [RequireFunction("ImportScreens")]
        public async Task<IActionResult> ImportScreens([FromForm] ImportScreensInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportScreensCommand command = new()
            {
                Data = stream,
                FileName = file.FileName
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<ScreenOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
