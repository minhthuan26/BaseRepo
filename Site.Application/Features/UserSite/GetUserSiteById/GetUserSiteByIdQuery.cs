using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.GetUserSiteById
{
    public class GetUserSiteByIdQuery : IRequest<UserSiteOutputDTO>
    {
        public string UserId { get; set; }
        public string SiteId { get; set; }
    }
}
