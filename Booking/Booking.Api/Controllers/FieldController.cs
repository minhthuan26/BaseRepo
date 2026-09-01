using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Booking.Application.Features.Field.CreateField;
using Booking.Application.Features.Field.DeleteField;
using Booking.Application.Features.Field.GetAllFields;
using Booking.Application.Features.Field.GetFieldById;
using Booking.Application.Features.Field.UpdateField;
using Booking.Contract.DTOs;

namespace Booking.Api.Controllers
{
    [Route("api/field")]
    [ApiController]
    public class FieldController : BaseController<FieldController>
    {
        public FieldController(ILogger<FieldController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("CreateField")]
        public async Task<IActionResult> CreateField([FromBody] FieldInputDTO data)
        {
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            data.SiteId = callerSiteId;
            var result = await _mediator.Send(new CreateFieldCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<FieldOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{fieldId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("UpdateField")]
        public async Task<IActionResult> UpdateField([FromRoute] string fieldId, [FromBody] FieldInputDTO data)
        {
            data.FieldId = fieldId;
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new UpdateFieldCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<FieldOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{fieldId}")]
        public async Task<IActionResult> GetField([FromRoute] string fieldId)
        {
            var result = await _mediator.Send(new GetFieldByIdQuery { Data = fieldId });
            return Ok(new Response<FieldOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllFields([FromQuery] FieldFilterDTO filter)
        {
            var result = await _mediator.Send(new GetAllFieldsQuery { Filter = filter });
            return Ok(new Response<BaseFilterOutput<FieldOutputDTO>>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{fieldId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("DeleteField")]
        public async Task<IActionResult> DeleteField([FromRoute] string fieldId, [FromBody] DeleteFieldInputDTO data)
        {
            data.FieldId = fieldId;
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new DeleteFieldCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<string>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
