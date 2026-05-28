using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommonLibs.Comtrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<T> : ControllerBase
    {
        protected readonly ILogger<T> _logger;
        protected readonly IMediator _mediator;
        protected readonly IHttpContextAccessor _httpContext;

        public BaseController(ILogger<T> logger, IMediator mediator, IHttpContextAccessor httpContext)
        {
            _logger = logger;
            _mediator = mediator;
            _httpContext = httpContext;
        }
    }
}
