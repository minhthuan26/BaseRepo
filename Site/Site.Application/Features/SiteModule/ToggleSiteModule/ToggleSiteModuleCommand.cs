using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.SiteModule.ToggleSiteModule
{
    public class ToggleSiteModuleCommand : IRequest<SiteModuleOutputDTO>
    {
        /// <summary>SiteId của người gọi (lấy từ claim "site_id", không phải giá trị client tự truyền) - xem
        /// SiteModuleController.ToggleSiteModule.</summary>
        public string SiteId { get; set; }
        public ToggleSiteModuleInputDTO Data { get; set; }
    }
}
