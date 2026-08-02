using CommonLibs.DTOs;
using Site.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface IUserSiteRepository
    {
        Task<UserSiteOutputDTO> CreateUserSite(UserSiteInputDTO input, CancellationToken cancellationToken);
        Task<UserSiteOutputDTO> GetUserSiteById(string userId, string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<UserSiteOutputDTO>> GetAllUserSites(UserSiteFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteUserSite(DeleteUserSiteInputDTO input, CancellationToken cancellationToken);
        Task<List<UserSiteOutputDTO>> ImportUserSites(Stream stream, string fileName, CancellationToken cancellationToken);
        Task<string> ExportUserSites(CancellationToken cancellationToken);
        Task<List<SiteOutputDTO>> GetSitesByUser(string userId, CancellationToken cancellationToken);
    }
}
