using CommonLibs.DTOs;
using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Site.GetAllSites
{
    public class GetAllSitesQuery : IRequest<BaseFilterOutput<SiteOutputDTO>>
    {
        public SiteFilterDTO Filter { get; set; }
    }
}
