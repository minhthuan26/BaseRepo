using MediatR;

namespace Site.Application.Features.Permission.AssignRolePermissions
{
    public class AssignRolePermissionsCommand : IRequest<List<string>>
    {
        public string RoleId { get; set; }
        public List<string> FunctionIds { get; set; } = new();
        // Site của người gọi (claim "site_id") - Controller luôn gán, KHÔNG đọc từ client.
        public string SiteId { get; set; }
    }
}
