using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Role.ImportRoles
{
    public class ImportRolesCommand : IRequest<List<RoleOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
        public string SiteId { get; set; }
    }
}
