using CommonLibs.Comtrollers;
using CommonLibs.Utils;
using Gateway.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Route("api/v1/gateway")]
    [ApiController]
    public class TestGatewayAPIController : BaseController<TestGatewayAPIController>
    {
        public TestGatewayAPIController(ILogger<TestGatewayAPIController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> TestGetAPI([FromRoute] string id, [FromQuery] string order)
        {
            return Ok(new Response<object>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Gateway GET successfully",
                Data = new
                {
                    Id = id,
                    Order = order
                }
            });
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> TestPostAPI([FromRoute] string id, [FromBody] string fullName)
        {
            return Ok(new Response<object>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Gateway POST successfully",
                Data = new
                {
                    Id = id,
                    FullName = fullName
                }
            });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> TestPatchAPI([FromRoute] string id, [FromBody] string fullName)
        {
            return Ok(new Response<object>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Gateway PATCH successfully",
                Data = new
                {
                    Id = id,
                    FullName = fullName
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> TestPutAPI([FromRoute] string id, [FromBody] string fullName)
        {
            return Ok(new Response<object>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Gateway PUT successfully",
                Data = new
                {
                    Id = id,
                    FullName = fullName
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> TestDeleteAPI([FromRoute] string id, [FromBody] string fullName)
        {
            return Ok(new Response<object>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Gateway DELETE successfully",
                Data = new
                {
                    Id = id,
                    FullName = fullName
                }
            });
        }
    }
}
