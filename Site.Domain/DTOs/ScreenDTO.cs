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
    public class ScreenInputDTO : BaseInputDTO
    {
        public string ScreenId { get; set; }
        public string ScreenName { get; set; }
        public string ModuleId { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["ScreenId"] = nameof(ScreenId),
            ["ScreenName"] = nameof(ScreenName),
            ["ModuleId"] = nameof(ModuleId),
            ["Status"] = nameof(Status)
        };
    }

    public class ImportScreensInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteScreenInputDTO : BaseInputDTO
    {
        public string? ScreenId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class ScreenOutputDTO : BaseOutputDTO
    {
        public string ScreenId { get; set; }
        public string ScreenName { get; set; }
        public string ModuleId { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class ScreenFilterDTO : BaseFilterDTO
    {
        public string? ScreenName { get; set; }
        public string? ModuleId { get; set; }
    }
    #endregion
}
