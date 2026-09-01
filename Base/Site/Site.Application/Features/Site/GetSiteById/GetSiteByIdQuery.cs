using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Site.GetSiteById
{
    public class GetSiteByIdQuery : IRequest<SiteOutputDTO>
    {
        public string Data { get; set; }
    }
}
