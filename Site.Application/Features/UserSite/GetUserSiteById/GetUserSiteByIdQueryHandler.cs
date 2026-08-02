using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.GetUserSiteById
{
    public class GetUserSiteByIdQueryHandler : IRequestHandler<GetUserSiteByIdQuery, UserSiteOutputDTO>
    {
        private readonly IUserSiteRepository _repository;

        public GetUserSiteByIdQueryHandler(IUserSiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserSiteOutputDTO> Handle(GetUserSiteByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetUserSiteById(request.UserId, request.SiteId, cancellationToken);
            return result;
        }
    }
}
