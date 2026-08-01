using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Site.CreateSite
{
    public class CreateSiteCommand : IRequest<SiteOutputDTO>
    {
        public SiteInputDTO Data { get; set; }
    }
}
