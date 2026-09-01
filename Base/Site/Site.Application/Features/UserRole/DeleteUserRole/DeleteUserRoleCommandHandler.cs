using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.UserRole.DeleteUserRole
{
    public class DeleteUserRoleCommandHandler : IRequestHandler<DeleteUserRoleCommand, string>
    {
        private readonly IUserRoleRepository _repository;

        public DeleteUserRoleCommandHandler(IUserRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteUserRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteUserRole(request.Data, cancellationToken);
            return result;
        }
    }
}
