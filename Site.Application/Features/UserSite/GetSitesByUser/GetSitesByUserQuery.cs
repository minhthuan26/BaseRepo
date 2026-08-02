using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.GetSitesByUser
{
    public class GetSitesByUserQuery : IRequest<List<SiteOutputDTO>>
    {
        public string UserId { get; set; }
    }
}
