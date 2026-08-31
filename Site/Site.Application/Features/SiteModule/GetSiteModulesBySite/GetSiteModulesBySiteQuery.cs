using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.SiteModule.GetSiteModulesBySite
{
    public class GetSiteModulesBySiteQuery : IRequest<List<SiteModuleOutputDTO>>
    {
        public string SiteId { get; set; }
    }
}
