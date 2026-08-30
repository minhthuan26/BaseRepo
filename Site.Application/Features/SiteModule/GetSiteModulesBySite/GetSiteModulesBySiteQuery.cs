using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.SiteModule.GetSiteModulesBySite
{
    public class GetSiteModulesBySiteQuery : IRequest<List<SiteModuleOutputDTO>>
    {
        public string SiteId { get; set; }
    }
}
