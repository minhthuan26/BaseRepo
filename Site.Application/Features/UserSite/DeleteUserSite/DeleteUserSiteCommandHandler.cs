using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.UserSite.DeleteUserSite
{
    public class DeleteUserSiteCommandHandler : IRequestHandler<DeleteUserSiteCommand, string>
    {
        private readonly IUserSiteRepository _repository;

        public DeleteUserSiteCommandHandler(IUserSiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteUserSiteCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteUserSite(request.Data, cancellationToken);
            return result;
        }
    }
}
