using MediatR;

namespace Site.Application.Features.UserRole.ExportUserRoles
{
    public class ExportUserRolesQuery : IRequest<string>
    {
        // Site của người gọi (claim "site_id") - bắt buộc, để export không trả chéo dữ liệu User-Role của Site
        // khác (trước đây export toàn bộ tbl_User_Role không lọc Site cho bất kỳ user đã đăng nhập nào).
        public string SiteId { get; set; }
    }
}
