using CommonLibs.Comtrollers;
using CommonLibs.Utils;
using Gateway.Application.Features.User.CreateUser;
using Gateway.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : BaseController<UserController>
    {
        public UserController(ILogger<UserController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateUser([FromBody] UserInputDTO data)
        {
            CreateUserCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<UserOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Created successfully",
                Data = result
            });
        }
    }
}
