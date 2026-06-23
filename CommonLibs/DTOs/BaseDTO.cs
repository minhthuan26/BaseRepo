using CommonLibs.Entities;

namespace CommonLibs.DTOs
{
    public class BaseFilterOutput<T>
    {
        public long? TotalRecords { get; set; }
        public long? TotalPages { get; set; }
        public long? PageNumber { get; set; }
        public long? PageSize { get; set; }
        public List<T> Records { get; set; } = new List<T>();
    }

    public class BaseOutputDTO
    {
        public long AutoId { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Status { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceId { get; set; }
        //public string? Type { get; set; }
    }

    public class BaseInputDTO
    {
        public DateTime? UpdatedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public EntityStatus? Status { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceId { get; set; }
        //public string? Type { get; set; }
        public virtual Dictionary<string, string> HeaderMappings => new();

        public static Dictionary<string, string> GetHeaders<T>()
            where T : BaseInputDTO, new()
        {
            return new T().HeaderMappings;
        }

        public virtual void SetAuditInfo(string? userName, string? deviceId, string? deviceName)
        {
            UpdatedDate = DateTime.UtcNow;
            UpdatedBy = userName;

            DeviceId = deviceId;
            DeviceName = deviceName;

            if (CreatedDate == default)
            {
                CreatedDate = UpdatedDate;
                CreatedBy = userName;
            }
        }
    }
}
