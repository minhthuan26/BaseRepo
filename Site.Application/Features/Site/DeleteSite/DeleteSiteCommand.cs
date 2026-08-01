using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Site.DeleteSite
{
    public class DeleteSiteCommand : IRequest<string>
    {
        public DeleteSiteInputDTO Data { get; set; }
    }
}
