using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Role.ExportRoles
{
    public class ExportRolesQueryHandler : IRequestHandler<ExportRolesQuery, string>
    {
        private readonly IRoleRepository _repository;

        public ExportRolesQueryHandler(IRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportRolesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportRoles(request.SiteId, cancellationToken);
            return result;
        }
    }
}
