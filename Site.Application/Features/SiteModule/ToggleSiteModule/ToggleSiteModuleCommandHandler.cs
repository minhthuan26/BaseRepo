using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.SiteModule.ToggleSiteModule
{
    public class ToggleSiteModuleCommandHandler : IRequestHandler<ToggleSiteModuleCommand, SiteModuleOutputDTO>
    {
        private readonly ISiteModuleRepository _repository;

        public ToggleSiteModuleCommandHandler(ISiteModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<SiteModuleOutputDTO> Handle(ToggleSiteModuleCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ToggleSiteModule(request.SiteId, request.Data, cancellationToken);
            return result;
        }
    }
}
