using CommonLibs.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Contract.DTOs
{
    #region Input DTOs
    public class RoleFunctionInputDTO : BaseInputDTO
    {
        public string RoleId { get; set; }
        public string FunctionId { get; set; }
        // Site của người gọi (claim "site_id") - Controller luôn ghi đè, KHÔNG đọc từ client. Dùng để xác nhận
        // RoleId được cấp Function thực sự thuộc đúng Site của người gọi.
        public string? SiteId { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["RoleId"] = nameof(RoleId),
            ["FunctionId"] = nameof(FunctionId),
            ["Status"] = nameof(Status)
        };
    }

    public class ImportRoleFunctionsInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteRoleFunctionInputDTO : BaseInputDTO
    {
        public string? RoleId { get; set; }
        public string? FunctionId { get; set; }
        // Site của người gọi (claim "site_id") - RoleId không còn unique toàn cục kể từ
        // 08_fix_roleid_unique_scope.sql, bắt buộc để không xoá nhầm RoleFunction của Role trùng RoleId ở Site khác.
        public string? SiteId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class RoleFunctionOutputDTO : BaseOutputDTO
    {
        public string RoleId { get; set; }
        public string FunctionId { get; set; }
        public string SiteId { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class RoleFunctionFilterDTO : BaseFilterDTO
    {
        public string? RoleId { get; set; }
        public string? FunctionId { get; set; }
        public string? SiteId { get; set; }
    }
    #endregion
}
