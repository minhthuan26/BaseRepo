namespace CommonLibs.Entities
{
    public class BaseEntity
    {
        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public EntityStatus Status { get; set; } = EntityStatus.Active;
        public string? DeviceName { get; set; }
        public string? DeviceId { get; set; }
        public EntityType Type { get; set; } = EntityType.Custom;
    }

    public enum EntityStatus
    {
        Active = 1,
        Inactive = 0,
        Deleted = -1
    }

    public enum EntityType
    {
        Base = 0,
        Custom = 1
    }
}
