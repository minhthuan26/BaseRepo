using CommonLibs.Entities;

namespace CommonLibs.DTOs
{
    public class BaseFilterDTO
    {
        public int? Status { get; set; } = 1; // Mặc định lấy các bản ghi đang hoạt động
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Sort? SortBy { get; set; } = Sort.AutoIdAsc;
    }
    public enum Sort
    {
        AutoIdAsc, //latest created first
        AutoIdDes, //newest created first
    }
}
