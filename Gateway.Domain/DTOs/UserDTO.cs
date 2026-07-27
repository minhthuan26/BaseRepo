using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Domain.DTOs
{
    public class UserOutputDTO : BaseOutputDTO
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public int Priority { get; set; }

        public string? RefreshToken { get; set; }

        public string? Signature { get; set; }

        public string? Email { get; set; }

        public string? QuickCode { get; set; }
    }

    public class UserInputDTO : BaseInputDTO
    {
        public string UserId { get; set; }
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

    public class UserFilterDTO : BaseFilterDTO
    {
        public int? Priority { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? QuickCode { get; set; }
    }

    public class ChangePasswordInputDTO
    {
        public string? UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
