using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.Site.UpdateSite
{
    public class UpdateSiteCommandHandler : IRequestHandler<UpdateSiteCommand, SiteOutputDTO>
    {
        private readonly ISiteRepository _repository;

        public UpdateSiteCommandHandler(ISiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<SiteOutputDTO> Handle(UpdateSiteCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.UpdateSite(request.Data, cancellationToken);
            return result;
        }
    }
}
