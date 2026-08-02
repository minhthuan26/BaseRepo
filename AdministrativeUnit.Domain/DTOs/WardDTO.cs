using CommonLibs.DTOs;
using Microsoft.AspNetCore.Http;

namespace AdministrativeUnit.Domain.DTOs
{
    #region Input DTOs
    public class WardInputDTO : BaseInputDTO
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; }
        public string? NameWithType { get; set; }
        public string ProvinceCode { get; set; }
        public string? Slug { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["Code"] = nameof(Code),
            ["Name"] = nameof(Name),
            ["Type"] = nameof(Type),
            ["NameWithType"] = nameof(NameWithType),
            ["ProvinceCode"] = nameof(ProvinceCode),
            ["Slug"] = nameof(Slug),
            ["Status"] = nameof(Status)
        };
    }

    public class ImportWardsInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteWardInputDTO : BaseInputDTO
    {
        public string? Code { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class WardOutputDTO : BaseOutputDTO
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; }
        public string? NameWithType { get; set; }
        public string ProvinceCode { get; set; }
        public string? Slug { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class WardFilterDTO : BaseFilterDTO
    {
        public string? Name { get; set; }
        public string? ProvinceCode { get; set; }
    }
    #endregion
}
