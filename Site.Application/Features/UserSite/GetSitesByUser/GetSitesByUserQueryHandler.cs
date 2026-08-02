using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.GetSitesByUser
{
    public class GetSitesByUserQueryHandler : IRequestHandler<GetSitesByUserQuery, List<SiteOutputDTO>>
    {
        private readonly IUserSiteRepository _repository;

        public GetSitesByUserQueryHandler(IUserSiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SiteOutputDTO>> Handle(GetSitesByUserQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetSitesByUser(request.UserId, cancellationToken);
            return result;
        }
    }
}
