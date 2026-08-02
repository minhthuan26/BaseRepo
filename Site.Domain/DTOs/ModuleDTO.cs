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
    public class ModuleInputDTO : BaseInputDTO
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string SiteId { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["ModuleId"] = nameof(ModuleId),
            ["ModuleName"] = nameof(ModuleName),
            ["SiteId"] = nameof(SiteId),
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
        public string SiteId { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class ModuleFilterDTO : BaseFilterDTO
    {
        public string? ModuleName { get; set; }
        public string? SiteId { get; set; }
    }
    #endregion
}
