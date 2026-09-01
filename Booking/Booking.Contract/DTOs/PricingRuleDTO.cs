using CommonLibs.DTOs;

namespace Booking.Contract.DTOs
{
    #region Input DTOs
    public class PricingRuleInputDTO : BaseInputDTO
    {
        public long AutoId { get; set; }
        public string FieldId { get; set; } = null!;
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal PricePerHour { get; set; }
    }

    public class DeletePricingRuleInputDTO : BaseInputDTO
    {
        public long AutoId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class PricingRuleOutputDTO : BaseOutputDTO
    {
        public string FieldId { get; set; } = null!;
        public string DayOfWeek { get; set; } = null!;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal PricePerHour { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class PricingRuleFilterDTO : BaseFilterDTO
    {
        public string? FieldId { get; set; }
        public int? DayOfWeek { get; set; }
    }
    #endregion
}
