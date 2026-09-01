using CommonLibs.DTOs;
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.GetAllPricingRules
{
    public class GetAllPricingRulesQueryHandler : IRequestHandler<GetAllPricingRulesQuery, BaseFilterOutput<PricingRuleOutputDTO>>
    {
        private readonly IPricingRuleRepository _repository;

        public GetAllPricingRulesQueryHandler(IPricingRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<PricingRuleOutputDTO>> Handle(GetAllPricingRulesQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllPricingRules(request.Filter, cancellationToken);
        }
    }
}
