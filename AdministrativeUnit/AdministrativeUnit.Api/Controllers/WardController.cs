using AdministrativeUnit.Application.Features.Ward.CreateWard;
using AdministrativeUnit.Application.Features.Ward.DeleteWard;
using AdministrativeUnit.Application.Features.Ward.ExportWards;
using AdministrativeUnit.Application.Features.Ward.GetAllWards;
using AdministrativeUnit.Application.Features.Ward.GetWardById;
using AdministrativeUnit.Application.Features.Ward.GetWardsByProvince;
using AdministrativeUnit.Application.Features.Ward.ImportWards;
using AdministrativeUnit.Application.Features.Ward.UpdateWard;
using AdministrativeUnit.Contract.DTOs;
using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdministrativeUnit.Api.Controllers
{
    [Route("api/ward")]
    [ApiController]
    public class WardController : BaseController<WardController>
    {
        public WardController(ILogger<WardController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("CreateWard")]
        public async Task<IActionResult> CreateWard([FromBody] WardInputDTO data)
        {
            CreateWardCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<WardOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{code}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("UpdateWard")]
        public async Task<IActionResult> UpdateWard([FromRoute] string code, [FromBody] WardInputDTO data)
        {
            data.Code = code;
            UpdateWardCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<WardOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetWard([FromRoute] string code)
        {
            GetWardByIdQuery query = new GetWardByIdQuery()
            {
                Data = code
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<WardOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("by-province/{provinceCode}")]
        public async Task<IActionResult> GetWardsByProvince([FromRoute] string provinceCode)
        {
            GetWardsByProvinceQuery query = new GetWardsByProvinceQuery()
            {
                ProvinceCode = provinceCode
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<List<WardOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllWards([FromQuery] WardFilterDTO filter)
        {
            GetAllWardsQuery query = new GetAllWardsQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<WardOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportWards()
        {
            ExportWardsQuery query = new();
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
        [RequireFunction("DeleteWard")]
        public async Task<IActionResult> DeleteWard([FromRoute] string code, [FromBody] DeleteWardInputDTO data)
        {
            data.Code = code;
            DeleteWardCommand command = new()
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

        [HttpPost("import-wards")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("ImportWards")]
        public async Task<IActionResult> ImportWards([FromForm] ImportWardsInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportWardsCommand command = new()
            {
                Data = stream,
                FileName = file.FileName
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<WardOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
