using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.Features.UserSite.CreateUserSite;
using Site.Application.Features.UserSite.DeleteUserSite;
using Site.Application.Features.UserSite.ExportUserSites;
using Site.Application.Features.UserSite.GetAllUserSites;
using Site.Application.Features.UserSite.GetSitesByUser;
using Site.Application.Features.UserSite.GetUserSiteById;
using Site.Application.Features.UserSite.ImportUserSites;
using Site.Domain.DTOs;

namespace Site.Controllers
{
    [Route("api/user-site")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class UserSiteController : BaseController<UserSiteController>
    {
        public UserSiteController(ILogger<UserSiteController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [RequireFunction("CreateUserSite")]
        public async Task<IActionResult> CreateUserSite([FromBody] UserSiteInputDTO data)
        {
            CreateUserSiteCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<UserSiteOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{userId}/{siteId}")]
        public async Task<IActionResult> GetUserSite([FromRoute] string userId, [FromRoute] string siteId)
        {
            GetUserSiteByIdQuery query = new GetUserSiteByIdQuery()
            {
                UserId = userId,
                SiteId = siteId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<UserSiteOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetSitesByUser([FromRoute] string userId)
        {
            GetSitesByUserQuery query = new GetSitesByUserQuery()
            {
                UserId = userId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<List<SiteOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllUserSites([FromQuery] UserSiteFilterDTO filter)
        {
            GetAllUserSitesQuery query = new GetAllUserSitesQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<UserSiteOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportUserSites()
        {
            ExportUserSitesQuery query = new();
            var result = await _mediator.Send(query);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{userId}/{siteId}")]
        [RequireFunction("DeleteUserSite")]
        public async Task<IActionResult> DeleteUserSite([FromRoute] string userId, [FromRoute] string siteId, [FromBody] DeleteUserSiteInputDTO data)
        {
            data.UserId = userId;
            data.SiteId = siteId;
            DeleteUserSiteCommand command = new()
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

        [HttpPost("import-user-sites")]
        [RequireFunction("ImportUserSites")]
        public async Task<IActionResult> ImportUserSites([FromForm] ImportUserSitesInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportUserSitesCommand command = new()
            {
                Data = stream,
                FileName = file.FileName
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<UserSiteOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
