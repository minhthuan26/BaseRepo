using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Role.ImportRoles
{
    public class ImportRolesCommandHandler : IRequestHandler<ImportRolesCommand, List<RoleOutputDTO>>
    {
        private readonly IRoleRepository _repository;

        public ImportRolesCommandHandler(IRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RoleOutputDTO>> Handle(ImportRolesCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportRoles(request.Data, request.FileName, request.SiteId, cancellationToken);
            return result;
        }
    }
}
