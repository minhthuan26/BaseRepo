using CommonLibs.DTOs;
using Booking.Contract.DTOs;

namespace Booking.Application.Interfaces
{
    public interface IPricingRuleRepository
    {
        Task<PricingRuleOutputDTO> CreatePricingRule(PricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken);
        Task<PricingRuleOutputDTO> UpdatePricingRule(PricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken);
        Task<PricingRuleOutputDTO> GetPricingRuleById(long autoId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<PricingRuleOutputDTO>> GetAllPricingRules(PricingRuleFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeletePricingRule(DeletePricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken);
    }
}
