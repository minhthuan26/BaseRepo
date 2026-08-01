using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Site.ImportSites
{
    public class ImportSitesCommand : IRequest<List<SiteOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
