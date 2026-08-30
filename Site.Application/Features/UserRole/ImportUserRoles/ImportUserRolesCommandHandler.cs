using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserRole.ImportUserRoles
{
    public class ImportUserRolesCommandHandler : IRequestHandler<ImportUserRolesCommand, List<UserRoleOutputDTO>>
    {
        private readonly IUserRoleRepository _repository;

        public ImportUserRolesCommandHandler(IUserRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserRoleOutputDTO>> Handle(ImportUserRolesCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportUserRoles(request.Data, request.FileName, request.SiteId, cancellationToken);
            return result;
        }
    }
}
