using CommonLibs.DTOs;
using Site.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface ISiteRepository
    {
        Task<SiteOutputDTO> CreateSite(SiteInputDTO input, CancellationToken cancellationToken);
        Task<SiteOutputDTO> UpdateSite(SiteInputDTO input, CancellationToken cancellationToken);
        Task<SiteOutputDTO> GetSiteById(string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<SiteOutputDTO>> GetAllSites(SiteFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteSite(DeleteSiteInputDTO input, CancellationToken cancellationToken);
        Task<List<SiteOutputDTO>> ImportSites(Stream stream, string fileName, CancellationToken cancellationToken);
        Task<string> ExportSites(CancellationToken cancellationToken);
    }
}
