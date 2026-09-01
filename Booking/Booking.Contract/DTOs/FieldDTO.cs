using CommonLibs.DTOs;

namespace Booking.Contract.DTOs
{
    #region Input DTOs
    public class FieldInputDTO : BaseInputDTO
    {
        public string FieldId { get; set; } = null!;
        public string SiteId { get; set; } = null!;
        public string FieldTypeId { get; set; } = null!;
        public string FieldName { get; set; } = null!;
        public int OperationalStatus { get; set; } = 1;

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["FieldId"] = nameof(FieldId),
            ["FieldTypeId"] = nameof(FieldTypeId),
            ["FieldName"] = nameof(FieldName),
            ["Status"] = nameof(Status)
        };
    }

    public class DeleteFieldInputDTO : BaseInputDTO
    {
        public string? FieldId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class FieldOutputDTO : BaseOutputDTO
    {
        public string FieldId { get; set; } = null!;
        public string SiteId { get; set; } = null!;
        public string FieldTypeId { get; set; } = null!;
        public string FieldName { get; set; } = null!;
        public string OperationalStatus { get; set; } = null!;
    }
    #endregion

    #region Filter DTOs
    public class FieldFilterDTO : BaseFilterDTO
    {
        public string? FieldName { get; set; }
        public string? FieldTypeId { get; set; }
        public string? SiteId { get; set; }
    }
    #endregion
}
