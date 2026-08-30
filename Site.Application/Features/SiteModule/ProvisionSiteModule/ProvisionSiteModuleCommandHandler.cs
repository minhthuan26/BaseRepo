using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.SiteModule.ProvisionSiteModule
{
    public class ProvisionSiteModuleCommandHandler : IRequestHandler<ProvisionSiteModuleCommand, SiteModuleOutputDTO>
    {
        private readonly ISiteModuleRepository _repository;

        public ProvisionSiteModuleCommandHandler(ISiteModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<SiteModuleOutputDTO> Handle(ProvisionSiteModuleCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ProvisionSiteModule(request.Data, cancellationToken);
            return result;
        }
    }
}
