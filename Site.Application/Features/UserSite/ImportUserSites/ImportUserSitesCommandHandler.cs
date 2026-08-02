using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.ImportUserSites
{
    public class ImportUserSitesCommandHandler : IRequestHandler<ImportUserSitesCommand, List<UserSiteOutputDTO>>
    {
        private readonly IUserSiteRepository _repository;

        public ImportUserSitesCommandHandler(IUserSiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserSiteOutputDTO>> Handle(ImportUserSitesCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportUserSites(request.Data, request.FileName, cancellationToken);
            return result;
        }
    }
}
