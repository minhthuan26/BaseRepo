using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Booking.Application.Features.FieldType.CreateFieldType;
using Booking.Application.Features.FieldType.DeleteFieldType;
using Booking.Application.Features.FieldType.GetAllFieldTypes;
using Booking.Application.Features.FieldType.GetFieldTypeById;
using Booking.Application.Features.FieldType.UpdateFieldType;
using Booking.Contract.DTOs;

namespace Booking.Api.Controllers
{
    [Route("api/field-type")]
    [ApiController]
    public class FieldTypeController : BaseController<FieldTypeController>
    {
        public FieldTypeController(ILogger<FieldTypeController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("CreateFieldType")]
        public async Task<IActionResult> CreateFieldType([FromBody] FieldTypeInputDTO data)
        {
            var result = await _mediator.Send(new CreateFieldTypeCommand { Data = data });
            return Ok(new Response<FieldTypeOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{fieldTypeId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("UpdateFieldType")]
        public async Task<IActionResult> UpdateFieldType([FromRoute] string fieldTypeId, [FromBody] FieldTypeInputDTO data)
        {
            data.FieldTypeId = fieldTypeId;
            var result = await _mediator.Send(new UpdateFieldTypeCommand { Data = data });
            return Ok(new Response<FieldTypeOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{fieldTypeId}")]
        public async Task<IActionResult> GetFieldType([FromRoute] string fieldTypeId)
        {
            var result = await _mediator.Send(new GetFieldTypeByIdQuery { Data = fieldTypeId });
            return Ok(new Response<FieldTypeOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllFieldTypes([FromQuery] FieldTypeFilterDTO filter)
        {
            var result = await _mediator.Send(new GetAllFieldTypesQuery { Filter = filter });
            return Ok(new Response<BaseFilterOutput<FieldTypeOutputDTO>>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{fieldTypeId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("DeleteFieldType")]
        public async Task<IActionResult> DeleteFieldType([FromRoute] string fieldTypeId, [FromBody] DeleteFieldTypeInputDTO data)
        {
            data.FieldTypeId = fieldTypeId;
            var result = await _mediator.Send(new DeleteFieldTypeCommand { Data = data });
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
