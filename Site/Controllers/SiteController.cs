using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.Features.Site.CreateSite;
using Site.Application.Features.Site.DeleteSite;
using Site.Application.Features.Site.ExportSites;
using Site.Application.Features.Site.GetAllSites;
using Site.Application.Features.Site.GetSiteById;
using Site.Application.Features.Site.ImportSites;
using Site.Application.Features.Site.UpdateSite;
using Site.Domain.DTOs;

namespace Site.Controllers
{
    [Route("api/site")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class SiteController : BaseController<SiteController>
    {
        public SiteController(ILogger<SiteController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [RequireFunction("CreateSite")]
        public async Task<IActionResult> CreateSite([FromBody] SiteInputDTO data)
        {
            CreateSiteCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<SiteOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{siteId}")]
        public async Task<IActionResult> UpdateSite([FromRoute] string siteId, [FromBody] SiteInputDTO data)
        {
            if (!IsSiteAdmin(siteId))
            {
                throw new AppException("Access denied. You must be an Admin of this site.", System.Net.HttpStatusCode.Forbidden);
            }

            data.SiteId = siteId;
            UpdateSiteCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<SiteOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{siteId}")]
        public async Task<IActionResult> GetSite([FromRoute] string siteId)
        {
            GetSiteByIdQuery query = new GetSiteByIdQuery()
            {
                Data = siteId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<SiteOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllSites([FromQuery] SiteFilterDTO filter)
        {
            GetAllSitesQuery query = new GetAllSitesQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<SiteOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportSites()
        {
            ExportSitesQuery query = new();
            var result = await _mediator.Send(query);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{siteId}")]
        public async Task<IActionResult> DeleteSite([FromRoute] string siteId, [FromBody] DeleteSiteInputDTO data)
        {
            if (!IsSiteAdmin(siteId))
            {
                throw new AppException("Access denied. You must be an Admin of this site.", System.Net.HttpStatusCode.Forbidden);
            }

            data.SiteId = siteId;
            DeleteSiteCommand command = new()
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

        [HttpPost("import-sites")]
        [RequireFunction("ImportSites")]
        public async Task<IActionResult> ImportSites([FromForm] ImportSitesInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportSitesCommand command = new()
            {
                Data = stream,
                FileName = file.FileName
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<SiteOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
