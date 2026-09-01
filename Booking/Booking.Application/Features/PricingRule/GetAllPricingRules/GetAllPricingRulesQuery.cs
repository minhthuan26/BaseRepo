using CommonLibs.DTOs;
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.GetAllPricingRules
{
    public class GetAllPricingRulesQuery : IRequest<BaseFilterOutput<PricingRuleOutputDTO>>
    {
        public PricingRuleFilterDTO Filter { get; set; } = null!;
    }
}
