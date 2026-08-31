using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.SiteModule.ProvisionSiteModule
{
    public class ProvisionSiteModuleCommand : IRequest<SiteModuleOutputDTO>
    {
        public ProvisionSiteModuleInputDTO Data { get; set; }
    }
}
