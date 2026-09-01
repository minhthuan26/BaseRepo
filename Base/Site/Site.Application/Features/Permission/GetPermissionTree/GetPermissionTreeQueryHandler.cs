using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Permission.GetPermissionTree
{
    public class GetPermissionTreeQueryHandler : IRequestHandler<GetPermissionTreeQuery, PermissionTreeDTO>
    {
        private readonly IPermissionRepository _repository;

        public GetPermissionTreeQueryHandler(IPermissionRepository repository)
        {
            _repository = repository;
        }

        public async Task<PermissionTreeDTO> Handle(GetPermissionTreeQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetPermissionTree(request.SiteId, request.RoleId, cancellationToken);
            return result;
        }
    }
}
