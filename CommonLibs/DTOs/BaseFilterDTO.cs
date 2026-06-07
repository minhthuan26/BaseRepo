using CommonLibs.Entities;

namespace CommonLibs.DTOs
{
    public class BaseFilterDTO
    {
        public int? PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public EntityStatus? Status { get; set; }
        //public EntityType? Type { get; set; }
    }
}
