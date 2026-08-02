using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.UserSite.ExportUserSites
{
    public class ExportUserSitesQueryHandler : IRequestHandler<ExportUserSitesQuery, string>
    {
        private readonly IUserSiteRepository _repository;

        public ExportUserSitesQueryHandler(IUserSiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportUserSitesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportUserSites(cancellationToken);
            return result;
        }
    }
}
