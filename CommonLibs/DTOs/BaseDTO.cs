namespace CommonLibs.DTOs
{
    public class BaseDTO
    {
        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Status { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceId { get; set; }
        public string? Type { get; set; }
    }
}
