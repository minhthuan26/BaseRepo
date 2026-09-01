using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Booking.Application.Features.PricingRule.CreatePricingRule;
using Booking.Application.Features.PricingRule.DeletePricingRule;
using Booking.Application.Features.PricingRule.GetAllPricingRules;
using Booking.Application.Features.PricingRule.GetPricingRuleById;
using Booking.Application.Features.PricingRule.UpdatePricingRule;
using Booking.Contract.DTOs;

namespace Booking.Api.Controllers
{
    [Route("api/pricing-rule")]
    [ApiController]
    public class PricingRuleController : BaseController<PricingRuleController>
    {
        public PricingRuleController(ILogger<PricingRuleController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("CreatePricingRule")]
        public async Task<IActionResult> CreatePricingRule([FromBody] PricingRuleInputDTO data)
        {
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new CreatePricingRuleCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<PricingRuleOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{autoId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("UpdatePricingRule")]
        public async Task<IActionResult> UpdatePricingRule([FromRoute] long autoId, [FromBody] PricingRuleInputDTO data)
        {
            data.AutoId = autoId;
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new UpdatePricingRuleCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<PricingRuleOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{autoId}")]
        public async Task<IActionResult> GetPricingRule([FromRoute] long autoId)
        {
            var result = await _mediator.Send(new GetPricingRuleByIdQuery { Data = autoId });
            return Ok(new Response<PricingRuleOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllPricingRules([FromQuery] PricingRuleFilterDTO filter)
        {
            var result = await _mediator.Send(new GetAllPricingRulesQuery { Filter = filter });
            return Ok(new Response<BaseFilterOutput<PricingRuleOutputDTO>>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{autoId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("DeletePricingRule")]
        public async Task<IActionResult> DeletePricingRule([FromRoute] long autoId, [FromBody] DeletePricingRuleInputDTO data)
        {
            data.AutoId = autoId;
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new DeletePricingRuleCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<string>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
