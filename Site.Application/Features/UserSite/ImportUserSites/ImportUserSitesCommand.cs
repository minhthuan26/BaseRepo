using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.ImportUserSites
{
    public class ImportUserSitesCommand : IRequest<List<UserSiteOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
