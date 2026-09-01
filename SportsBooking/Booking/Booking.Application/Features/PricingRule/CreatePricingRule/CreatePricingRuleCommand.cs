using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.CreatePricingRule
{
    public class CreatePricingRuleCommand : IRequest<PricingRuleOutputDTO>
    {
        public PricingRuleInputDTO Data { get; set; } = null!;
        public string CallerSiteId { get; set; } = null!;
    }
}
