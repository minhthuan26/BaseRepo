using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserRole.ImportUserRoles
{
    public class ImportUserRolesCommand : IRequest<List<UserRoleOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
