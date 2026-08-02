using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.Role.CreateRole
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleOutputDTO>
    {
        private readonly IRoleRepository _repository;

        public CreateRoleCommandHandler(IRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<RoleOutputDTO> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateRole(request.Data, cancellationToken);
            return result;
        }
    }
}
