using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace Booking.Domain.Entities
{
    public class PricingRule : BaseEntity
    {
        public string FieldId { get; private set; } = null!;
        public DayOfWeek DayOfWeek { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public decimal PricePerHour { get; private set; }

        protected PricingRule()
        {
        }

        public PricingRule(string fieldId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, decimal pricePerHour)
        {
            SetFieldId(fieldId);
            SetDayOfWeek(dayOfWeek);
            SetTimeRange(startTime, endTime);
            SetPricePerHour(pricePerHour);
        }

        public PricingRule(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetFieldId(row.Field<string>("FieldId") ?? throw new AppException("FieldId is required"));
            SetDayOfWeek((DayOfWeek)row.Field<int>("DayOfWeek"));
            SetTimeRange(row.Field<TimeSpan>("StartTime"), row.Field<TimeSpan>("EndTime"));
            SetPricePerHour(row.Field<decimal>("PricePerHour"));
        }

        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetFieldId(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new AppException("FieldId could not be empty");

            FieldId = fieldId.Trim();
        }

        public void SetDayOfWeek(DayOfWeek dayOfWeek)
        {
            DayOfWeek = dayOfWeek;
        }

        public void SetTimeRange(TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime <= startTime)
                throw new AppException("EndTime must be after StartTime");

            StartTime = startTime;
            EndTime = endTime;
        }

        public void SetPricePerHour(decimal pricePerHour)
        {
            if (pricePerHour <= 0)
                throw new AppException("PricePerHour must be greater than 0");

            PricePerHour = pricePerHour;
        }
    }
}
