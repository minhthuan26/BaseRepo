using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Permission.GetPermissionTree
{
    public class GetPermissionTreeQuery : IRequest<PermissionTreeDTO>
    {
        public string SiteId { get; set; }
        public string? RoleId { get; set; }
    }
}
