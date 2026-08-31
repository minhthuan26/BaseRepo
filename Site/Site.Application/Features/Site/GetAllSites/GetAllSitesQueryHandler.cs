using CommonLibs.DTOs;
using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Site.GetAllSites
{
    public class GetAllSitesQueryHandler : IRequestHandler<GetAllSitesQuery, BaseFilterOutput<SiteOutputDTO>>
    {
        private readonly ISiteRepository _repository;

        public GetAllSitesQueryHandler(ISiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<SiteOutputDTO>> Handle(GetAllSitesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllSites(request.Filter, cancellationToken);
            return result;
        }
    }
}
