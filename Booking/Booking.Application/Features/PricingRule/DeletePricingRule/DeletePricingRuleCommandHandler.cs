using MediatR;
using Booking.Application.Interfaces;

namespace Booking.Application.Features.PricingRule.DeletePricingRule
{
    public class DeletePricingRuleCommandHandler : IRequestHandler<DeletePricingRuleCommand, string>
    {
        private readonly IPricingRuleRepository _repository;

        public DeletePricingRuleCommandHandler(IPricingRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeletePricingRuleCommand request, CancellationToken cancellationToken)
        {
            return await _repository.DeletePricingRule(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
