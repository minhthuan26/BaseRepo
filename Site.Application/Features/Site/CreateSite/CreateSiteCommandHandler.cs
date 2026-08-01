using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.Site.CreateSite
{
    public class CreateSiteCommandHandler : IRequestHandler<CreateSiteCommand, SiteOutputDTO>
    {
        private readonly ISiteRepository _repository;

        public CreateSiteCommandHandler(ISiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<SiteOutputDTO> Handle(CreateSiteCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateSite(request.Data, cancellationToken);
            return result;
        }
    }
}
