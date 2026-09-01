using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.CreatePricingRule
{
    public class CreatePricingRuleCommandHandler : IRequestHandler<CreatePricingRuleCommand, PricingRuleOutputDTO>
    {
        private readonly IPricingRuleRepository _repository;

        public CreatePricingRuleCommandHandler(IPricingRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<PricingRuleOutputDTO> Handle(CreatePricingRuleCommand request, CancellationToken cancellationToken)
        {
            return await _repository.CreatePricingRule(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
