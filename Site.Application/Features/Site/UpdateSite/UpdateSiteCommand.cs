using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Site.UpdateSite
{
    public class UpdateSiteCommand : IRequest<SiteOutputDTO>
    {
        public SiteInputDTO Data { get; set; }
    }
}
