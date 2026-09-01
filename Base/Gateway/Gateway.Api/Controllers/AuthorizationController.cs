using Gateway.Contract.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommonLibs.Utils;
using CommonLibs.Comtrollers;
using Gateway.Application.Features.Authorization.Login;
using Gateway.Application.Features.Authorization.Logout;
using Gateway.Application.Features.Authorization.RenewToken;

namespace Gateway.Api.Controllers
{
    [Route("api/authorization")]
    [ApiController]
    public class AuthorizationController : BaseController<AuthorizationController>
    {
        public AuthorizationController(ILogger<AuthorizationController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {

        }

        /// <summary>
        /// Thực hiện đăng nhập, trả về access token và refresh token. Refresh token sẽ được lưu trong cookie HttpOnly để bảo mật hơn. Access token sẽ được trả về trong response body để client có thể sử dụng cho các yêu cầu tiếp theo.
        /// </summary>
        /// <param name="data">Truyền UserId và Password</param>
        /// <returns></returns>
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

        /// <summary>
        /// Thực hiện đăng xuất, xóa refresh token khỏi cookie để đảm bảo rằng người dùng không thể sử dụng refresh token cũ để lấy access token mới sau khi đã đăng xuất. Điều này giúp tăng cường bảo mật cho hệ thống của bạn bằng cách ngăn chặn việc sử dụng lại refresh token sau khi người dùng đã đăng xuất.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Làm mới access token bằng refresh token. Client gửi kèm access token cũ (đã hết hạn) qua header
        /// Authorization và refresh token qua cookie HttpOnly. Nếu hợp lệ, trả về access token + refresh token
        /// mới và cập nhật lại cookie RefreshToken.
        /// </summary>
        /// <returns></returns>
        [HttpPost("renew-token")]
        public async Task<IActionResult> RenewToken()
        {
            RenewTokenCommand command = new();
            var result = await _mediator.Send(command);
            // Lưu refresh token mới vào cookie
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
                Message = "Renew token successfully",
                Data = result
            });
        }
    }
}
