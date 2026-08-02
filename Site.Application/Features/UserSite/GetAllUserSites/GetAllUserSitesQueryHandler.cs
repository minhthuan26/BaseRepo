using CommonLibs.DTOs;
using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.GetAllUserSites
{
    public class GetAllUserSitesQueryHandler : IRequestHandler<GetAllUserSitesQuery, BaseFilterOutput<UserSiteOutputDTO>>
    {
        private readonly IUserSiteRepository _repository;

        public GetAllUserSitesQueryHandler(IUserSiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<UserSiteOutputDTO>> Handle(GetAllUserSitesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllUserSites(request.Filter, cancellationToken);
            return result;
        }
    }
}
