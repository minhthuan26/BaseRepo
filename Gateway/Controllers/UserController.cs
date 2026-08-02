using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using Gateway.Application.Features.User.ChangePassword;
using Gateway.Application.Features.User.CreateUser;
using Gateway.Application.Features.User.DeleteUser;
using Gateway.Application.Features.User.ExportUsers;
using Gateway.Application.Features.User.ImportUsers;
using Gateway.Application.Features.User.GetAllUsers;
using Gateway.Application.Features.User.GetUserById;
using Gateway.Application.Features.User.UpdateUser;
using Gateway.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gateway.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public class UserController : BaseController<UserController>
    {
        public UserController(ILogger<UserController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [RequireFunction("CreateUser")]
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
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{userId}")]
        [RequireFunction("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromRoute] string userId, [FromBody] UserInputDTO data)
        {
            data.UserId = userId;
            UpdateUserCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<UserOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser([FromRoute] string userId)
        {
            GetUserByIdQuery query = new GetUserByIdQuery()
            {
                Data = userId
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<UserOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserFilterDTO filter)
        {
            GetAllUsersQuery query = new GetAllUsersQuery()
            {
                Filter = filter
            };
            var result = await _mediator.Send(query);
            return Ok(new Response<BaseFilterOutput<UserOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
        
        [HttpGet("export")]
        public async Task<IActionResult> ExportUsers()
        {
            ExportUsersQuery query = new();
            var result = await _mediator.Send(query);
            return Ok(new Response<string>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{userId}")]
        [RequireFunction("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromRoute] string userId, [FromBody] DeleteUserInputDTO data)
        {
            data.UserId = userId;
            DeleteUserCommand command = new()
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

        [HttpPost("import-users")]
        [RequireFunction("ImportUsers")]
        public async Task<IActionResult> ImportUsers([FromForm] ImportUsersInputDTO data)
        {
            var file = data.File;
            if (file == null || file.Length == 0)
            {
                throw new AppException("File is required.", System.Net.HttpStatusCode.BadRequest);
            }

            using var stream = file.OpenReadStream();
            ImportUsersCommand command = new()
            {
                Data = stream,
                FileName = file.FileName
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<List<UserOutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPost("{userId}/change-password")]
        public async Task<IActionResult> ChangePassword([FromRoute] string userId, [FromBody] ChangePasswordInputDTO data)
        {
            data.UserId = userId;
            ChangePasswordCommand command = new()
            {
                Data = data
            };
            var result = await _mediator.Send(command);
            return Ok(new Response<UserOutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
