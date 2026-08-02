using AdministrativeUnit.Application.Features.Province.CreateProvince;
using AdministrativeUnit.Application.Features.Province.DeleteProvince;
using AdministrativeUnit.Application.Features.Province.ExportProvinces;
using AdministrativeUnit.Application.Features.Province.GetAllProvinces;
using AdministrativeUnit.Application.Features.Province.GetProvinceById;
using AdministrativeUnit.Application.Features.Province.ImportProvinces;
using AdministrativeUnit.Application.Features.Province.UpdateProvince;
using AdministrativeUnit.Domain.DTOs;
using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdministrativeUnit.Controllers
{
    [Route("api/province")]
    [ApiController]
    public class ProvinceController : BaseController<ProvinceController>
    {
        public ProvinceController(ILogger<ProvinceController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("CreateProvince")]
        public async Task<IActionResult> CreateProvince([FromBody] ProvinceInputDTO data)
        {
            CreateProvinceCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<ProvinceOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{code}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("UpdateProvince")]
        public async Task<IActionResult> UpdateProvince([FromRoute] string code, [FromBody] ProvinceInputDTO data)
        {
            data.Code = code;
            UpdateProvinceCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<ProvinceOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetProvince([FromRoute] string code)
        {
            GetProvinceByIdQuery query = new GetProvinceByIdQuery()
            {
                Data = code
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<ProvinceOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllProvinces([FromQuery] ProvinceFilterDTO filter)
        {
            GetAllProvincesQuery query = new GetAllProvincesQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<ProvinceOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportProvinces()
        {
            ExportProvincesQuery query = new();
            var result = await _mediator.Send(query);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("DeleteProvince")]
        public async Task<IActionResult> DeleteProvince([FromRoute] string code, [FromBody] DeleteProvinceInputDTO data)
        {
            data.Code = code;
            DeleteProvinceCommand command = new()
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

        [HttpPost("import-provinces")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("ImportProvinces")]
        public async Task<IActionResult> ImportProvinces([FromForm] ImportProvincesInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportProvincesCommand command = new()
            {
                Data = stream,
                FileName = file.FileName
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<ProvinceOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
