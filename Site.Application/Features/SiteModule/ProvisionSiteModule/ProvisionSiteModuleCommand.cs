using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.SiteModule.ProvisionSiteModule
{
    public class ProvisionSiteModuleCommand : IRequest<SiteModuleOutputDTO>
    {
        public ProvisionSiteModuleInputDTO Data { get; set; }
    }
}
