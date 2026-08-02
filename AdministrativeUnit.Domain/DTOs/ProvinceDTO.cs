using CommonLibs.DTOs;
using Microsoft.AspNetCore.Http;

namespace AdministrativeUnit.Domain.DTOs
{
    #region Input DTOs
    public class ProvinceInputDTO : BaseInputDTO
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; }
        public string? NameWithType { get; set; }
        public string? Slug { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["Code"] = nameof(Code),
            ["Name"] = nameof(Name),
            ["Type"] = nameof(Type),
            ["NameWithType"] = nameof(NameWithType),
            ["Slug"] = nameof(Slug),
            ["Status"] = nameof(Status)
        };
    }

    public class ImportProvincesInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteProvinceInputDTO : BaseInputDTO
    {
        public string? Code { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class ProvinceOutputDTO : BaseOutputDTO
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; }
        public string? NameWithType { get; set; }
        public string? Slug { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class ProvinceFilterDTO : BaseFilterDTO
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
    }
    #endregion
}
