using CommonLibs.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Contract.DTOs
{
    #region Input DTOs
    // SiteId trên các DTO dưới đây LUÔN bị Controller ghi đè bằng claim "site_id" của người gọi (CurrentSiteId)
    // trước khi vào tới Repository - KHÔNG bao giờ tin theo giá trị client tự truyền lên (body/query/CSV import).
    // tbl_Users.SiteId là ranh giới tenant (mỗi User chỉ thuộc đúng 1 Site, UserId chỉ unique trong phạm vi 1
    // Site) - thiếu điều kiện này ở bất kỳ đâu trong CRUD User là có thể đụng/ghi đè nhầm User trùng UserId ở
    // Site khác. Xem CommonLibs/BaseSQL/multi-tenant/README.md.
    public class UserInputDTO : BaseInputDTO
    {
        public string UserId { get; set; }
        public string SiteId { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public int Priority { get; set; } = 1;
        public string? RefreshToken { get; set; }
        public string? Signature { get; set; }
        public string? Email { get; set; }
        public string? QuickCode { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["UserId"] = nameof(UserId),
            ["UserName"] = nameof(UserName),
            ["Password"] = nameof(Password),
            ["Priority"] = nameof(Priority),
            ["Signature"] = nameof(Signature),
            ["Email"] = nameof(Email),
            ["QuickCode"] = nameof(QuickCode),
            ["Status"] = nameof(Status)
        };
    }

    public class ChangePasswordInputDTO : BaseInputDTO
    {
        public string? UserId { get; set; }
        public string? SiteId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }

    public class ImportUsersInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteUserInputDTO : BaseInputDTO
    {
        public string? UserId { get; set; }
        public string? SiteId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class UserOutputDTO : BaseOutputDTO
    {
        public string UserId { get; set; }

        public string SiteId { get; set; }

        public string UserName { get; set; }

        public int Priority { get; set; }

        public string? RefreshToken { get; set; }

        public string? Signature { get; set; }

        public string? Email { get; set; }

        public string? QuickCode { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class UserFilterDTO : BaseFilterDTO
    {
        public string? SiteId { get; set; }
        public int? Priority { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? QuickCode { get; set; }
    }
    #endregion
}
