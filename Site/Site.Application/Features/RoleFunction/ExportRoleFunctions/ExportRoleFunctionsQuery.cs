using MediatR;

namespace Site.Application.Features.RoleFunction.ExportRoleFunctions
{
    public class ExportRoleFunctionsQuery : IRequest<string>
    {
        public string SiteId { get; set; }
    }
}
