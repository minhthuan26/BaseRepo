using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.GetPricingRuleById
{
    public class GetPricingRuleByIdQueryHandler : IRequestHandler<GetPricingRuleByIdQuery, PricingRuleOutputDTO>
    {
        private readonly IPricingRuleRepository _repository;

        public GetPricingRuleByIdQueryHandler(IPricingRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<PricingRuleOutputDTO> Handle(GetPricingRuleByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetPricingRuleById(request.Data, cancellationToken);
        }
    }
}
