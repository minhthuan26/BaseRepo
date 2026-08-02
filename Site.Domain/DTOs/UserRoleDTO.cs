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
    }
    #endregion
}
