using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.UpdatePricingRule
{
    public class UpdatePricingRuleCommandHandler : IRequestHandler<UpdatePricingRuleCommand, PricingRuleOutputDTO>
    {
        private readonly IPricingRuleRepository _repository;

        public UpdatePricingRuleCommandHandler(IPricingRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<PricingRuleOutputDTO> Handle(UpdatePricingRuleCommand request, CancellationToken cancellationToken)
        {
            return await _repository.UpdatePricingRule(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
