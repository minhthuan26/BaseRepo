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
    public class RoleInputDTO : BaseInputDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string SiteId { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["RoleId"] = nameof(RoleId),
            ["RoleName"] = nameof(RoleName),
            ["SiteId"] = nameof(SiteId),
            ["Status"] = nameof(Status)
        };
    }

    public class ImportRolesInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteRoleInputDTO : BaseInputDTO
    {
        public string? RoleId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class RoleOutputDTO : BaseOutputDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string SiteId { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class RoleFilterDTO : BaseFilterDTO
    {
        public string? RoleName { get; set; }
        public string? SiteId { get; set; }
    }
    #endregion
}
