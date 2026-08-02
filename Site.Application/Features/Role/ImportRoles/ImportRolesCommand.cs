using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Role.ImportRoles
{
    public class ImportRolesCommand : IRequest<List<RoleOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
