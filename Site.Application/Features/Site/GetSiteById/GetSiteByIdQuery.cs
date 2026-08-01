using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Site.GetSiteById
{
    public class GetSiteByIdQuery : IRequest<SiteOutputDTO>
    {
        public string Data { get; set; }
    }
}
