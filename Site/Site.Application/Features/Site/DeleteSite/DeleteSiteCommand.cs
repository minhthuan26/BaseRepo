using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Site.DeleteSite
{
    public class DeleteSiteCommand : IRequest<string>
    {
        public DeleteSiteInputDTO Data { get; set; }
    }
}
