using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Site.ExportSites
{
    public class ExportSitesQueryHandler : IRequestHandler<ExportSitesQuery, string>
    {
        private readonly ISiteRepository _repository;

        public ExportSitesQueryHandler(ISiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportSitesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportSites(cancellationToken);
            return result;
        }
    }
}
