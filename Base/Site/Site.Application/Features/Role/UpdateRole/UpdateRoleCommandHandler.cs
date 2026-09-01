using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Role.UpdateRole
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleOutputDTO>
    {
        private readonly IRoleRepository _repository;

        public UpdateRoleCommandHandler(IRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<RoleOutputDTO> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.UpdateRole(request.Data, cancellationToken);
            return result;
        }
    }
}
