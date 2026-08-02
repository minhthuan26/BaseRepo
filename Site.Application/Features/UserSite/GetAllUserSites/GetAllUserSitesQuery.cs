using CommonLibs.DTOs;
using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.GetAllUserSites
{
    public class GetAllUserSitesQuery : IRequest<BaseFilterOutput<UserSiteOutputDTO>>
    {
        public UserSiteFilterDTO Filter { get; set; }
    }
}
