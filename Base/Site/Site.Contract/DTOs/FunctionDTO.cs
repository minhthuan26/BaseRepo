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
    public class FunctionInputDTO : BaseInputDTO
    {
        public string FunctionId { get; set; }
        public string FunctionName { get; set; }
        public string ScreenId { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["FunctionId"] = nameof(FunctionId),
            ["FunctionName"] = nameof(FunctionName),
            ["ScreenId"] = nameof(ScreenId),
            ["Status"] = nameof(Status)
        };
    }

    public class ImportFunctionsInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteFunctionInputDTO : BaseInputDTO
    {
        public string? FunctionId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class FunctionOutputDTO : BaseOutputDTO
    {
        public string FunctionId { get; set; }
        public string FunctionName { get; set; }
        public string ScreenId { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class FunctionFilterDTO : BaseFilterDTO
    {
        public string? FunctionName { get; set; }
        public string? ScreenId { get; set; }
    }
    #endregion
}
