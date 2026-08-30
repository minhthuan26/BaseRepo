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
    // Module là danh mục dùng chung toàn hệ thống kể từ migration 04_globalize_module_catalog.sql (xem
    // CommonLibs/BaseSQL/multi-tenant/README.md) - KHÔNG còn SiteId ở entity/DTO này nữa. Muốn biết/quản lý Site
    // nào đang dùng/bật Module nào, dùng SiteModuleController/SiteModuleRepository (bảng tbl_Site_Module).
    public class ModuleInputDTO : BaseInputDTO
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["ModuleId"] = nameof(ModuleId),
            ["ModuleName"] = nameof(ModuleName),
            ["Status"] = nameof(Status)
        };
    }

    public class ImportModulesInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteModuleInputDTO : BaseInputDTO
    {
        public string? ModuleId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class ModuleOutputDTO : BaseOutputDTO
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class ModuleFilterDTO : BaseFilterDTO
    {
        public string? ModuleName { get; set; }
    }
    #endregion
}
