using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.UserRole.ImportUserRoles
{
    public class ImportUserRolesCommand : IRequest<List<UserRoleOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
        // Site của người gọi (claim "site_id") - chỉ cho import các dòng UserId/RoleId thuộc đúng Site này, chặn
        // trường hợp người import (dù có quyền Function "ImportUserRoles") cố tình nhét RoleId của Site khác vào
        // file CSV để gán quyền chéo Site.
        public string SiteId { get; set; }
    }
}
