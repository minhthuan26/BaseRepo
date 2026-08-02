using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Role.DeleteRole
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, string>
    {
        private readonly IRoleRepository _repository;

        public DeleteRoleCommandHandler(IRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteRole(request.Data, cancellationToken);
            return result;
        }
    }
}
