using CommonLibs.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
    #region Input DTOs
    public class UserRoleInputDTO : BaseInputDTO
    {
        public string RoleId { get; set; }
        public string UserId { get; set; }
        // Site của người gọi (claim "site_id") - Controller luôn ghi đè, KHÔNG đọc từ client. Dùng để xác nhận
        // RoleId được gán thực sự thuộc đúng Site của người gọi (không cho Site A gán quyền dùng RoleId của Site
        // B dù có quyền Function CreateUserRole, nếu tình cờ biết được RoleId đó).
        public string? SiteId { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["UserId"] = nameof(UserId),
            ["RoleId"] = nameof(RoleId),
            ["Status"] = nameof(Status)
        };
    }

    public class ImportUserRolesInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteUserRoleInputDTO : BaseInputDTO
    {
        public string? UserId { get; set; }
        public string? RoleId { get; set; }
        // Site của người gọi (claim "site_id") - Controller luôn ghi đè, KHÔNG đọc từ client. Bắt buộc để
        // Repository resolve đúng Role của đúng Site người gọi (RoleId không còn unique toàn cục kể từ
        // 08_fix_roleid_unique_scope.sql) thay vì phải đoán mò/ambiguous chỉ bằng RoleId.
        public string? SiteId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class UserRoleOutputDTO : BaseOutputDTO
    {
        public string RoleId { get; set; }
        public string UserId { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class UserRoleFilterDTO : BaseFilterDTO
    {
        public string? UserId { get; set; }
        public string? RoleId { get; set; }
        // Luôn bị Controller ghi đè bằng SiteId của người gọi (claim "site_id") trước khi gửi xuống Repository -
        // KHÔNG bao giờ tin theo giá trị client tự truyền lên query string. Bắt buộc (không phải optional filter
        // như UserId/RoleId) để endpoint danh sách này không bao giờ trả chéo dữ liệu User-Role của Site khác.
        public string? SiteId { get; set; }
    }
    #endregion
}
