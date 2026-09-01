using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Site.ImportSites
{
    public class ImportSitesCommandHandler : IRequestHandler<ImportSitesCommand, List<SiteOutputDTO>>
    {
        private readonly ISiteRepository _repository;

        public ImportSitesCommandHandler(ISiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SiteOutputDTO>> Handle(ImportSitesCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportSites(request.Data, request.FileName, cancellationToken);
            return result;
        }
    }
}
