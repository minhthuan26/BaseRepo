using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Site.CreateSite
{
    public class CreateSiteCommand : IRequest<SiteOutputDTO>
    {
        public SiteInputDTO Data { get; set; }
    }
}
