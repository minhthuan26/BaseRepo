using CommonLibs.DTOs;

namespace Booking.Contract.DTOs
{
    #region Input DTOs
    public class FieldTypeInputDTO : BaseInputDTO
    {
        public string FieldTypeId { get; set; } = null!;
        public string FieldTypeName { get; set; } = null!;

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["FieldTypeId"] = nameof(FieldTypeId),
            ["FieldTypeName"] = nameof(FieldTypeName),
            ["Status"] = nameof(Status)
        };
    }

    public class DeleteFieldTypeInputDTO : BaseInputDTO
    {
        public string? FieldTypeId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class FieldTypeOutputDTO : BaseOutputDTO
    {
        public string FieldTypeId { get; set; } = null!;
        public string FieldTypeName { get; set; } = null!;
    }
    #endregion

    #region Filter DTOs
    public class FieldTypeFilterDTO : BaseFilterDTO
    {
        public string? FieldTypeName { get; set; }
    }
    #endregion
}
