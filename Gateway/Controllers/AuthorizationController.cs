using Gateway.Application.Commands;
using Gateway.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommonLibs.Utils;
using CommonLibs.Comtrollers;

namespace Gateway.Controllers.Authorization
{
    [Route("api/v1/authorization")]
    [ApiController]
    public class AuthorizationController : BaseController<AuthorizationController>
    {
        public AuthorizationController(ILogger<AuthorizationController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthorizationInputDTO data)
        {
            LoginCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            // Lưu refresh token vào cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = false,
                Path = "/"
            };
            _httpContext.HttpContext?.Response.Cookies.Append("RefreshToken", result.RefreshToken, cookieOptions);
            return Ok(new Response<AuthorizationOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Login successfully",
                Data = result
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            LogoutCommand command = new();
            var result = await _mediator.Send(command);
            // Lưu refresh token vào cookie
            _httpContext.HttpContext?.Response.Cookies.Delete(
                 "RefreshToken",
                 new CookieOptions
                 {
                     Path = "/",
                     HttpOnly = true,
                     Secure = false,
                     SameSite = SameSiteMode.Strict
                 }
             );
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Logout successfully",
                Data = result
            });
        }
    }
}
