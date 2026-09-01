using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.GetPricingRuleById
{
    public class GetPricingRuleByIdQuery : IRequest<PricingRuleOutputDTO>
    {
        public long Data { get; set; }
    }
}
