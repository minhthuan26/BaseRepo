using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Permission.AssignRolePermissions
{
    public class AssignRolePermissionsCommandHandler : IRequestHandler<AssignRolePermissionsCommand, List<string>>
    {
        private readonly IPermissionRepository _repository;

        public AssignRolePermissionsCommandHandler(IPermissionRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<string>> Handle(AssignRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.AssignRolePermissions(request.RoleId, request.FunctionIds, cancellationToken);
            return result;
        }
    }
}
