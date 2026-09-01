using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.UserRole.ExportUserRoles
{
    public class ExportUserRolesQueryHandler : IRequestHandler<ExportUserRolesQuery, string>
    {
        private readonly IUserRoleRepository _repository;

        public ExportUserRolesQueryHandler(IUserRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportUserRolesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportUserRoles(request.SiteId, cancellationToken);
            return result;
        }
    }
}
