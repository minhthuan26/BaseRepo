using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.SiteModule.GetSiteModulesBySite
{
    public class GetSiteModulesBySiteQueryHandler : IRequestHandler<GetSiteModulesBySiteQuery, List<SiteModuleOutputDTO>>
    {
        private readonly ISiteModuleRepository _repository;

        public GetSiteModulesBySiteQueryHandler(ISiteModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SiteModuleOutputDTO>> Handle(GetSiteModulesBySiteQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetSiteModulesBySite(request.SiteId, cancellationToken);
            return result;
        }
    }
}
