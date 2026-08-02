using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.CreateUserSite
{
    public class CreateUserSiteCommandHandler : IRequestHandler<CreateUserSiteCommand, UserSiteOutputDTO>
    {
        private readonly IUserSiteRepository _repository;

        public CreateUserSiteCommandHandler(IUserSiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserSiteOutputDTO> Handle(CreateUserSiteCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateUserSite(request.Data, cancellationToken);
            return result;
        }
    }
}
