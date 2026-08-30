using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserRole.GetRolesByUser
{
    public class GetRolesByUserQuery : IRequest<List<RoleOutputDTO>>
    {
        public string UserId { get; set; }
        // Site của người gọi (lấy từ claim "site_id" tại Controller, KHÔNG cho client tự truyền SiteId) - UserId
        // không còn unique toàn cục nên bắt buộc phải khoanh vùng đúng Site mới trả đúng Role, tránh trả nhầm
        // Role của 1 User trùng UserId ở Site khác.
        public string SiteId { get; set; }
    }
}
