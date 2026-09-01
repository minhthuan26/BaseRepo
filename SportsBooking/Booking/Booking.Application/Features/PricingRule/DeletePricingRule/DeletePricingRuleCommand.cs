using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.DeletePricingRule
{
    public class DeletePricingRuleCommand : IRequest<string>
    {
        public DeletePricingRuleInputDTO Data { get; set; } = null!;
        public string CallerSiteId { get; set; } = null!;
    }
}
