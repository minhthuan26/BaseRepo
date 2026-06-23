using CommonLibs.DTOs;
using Site.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface IUserFunctionRepository
    {
        Task<UserSiteOutputDTO> CreateUserSite(UserSiteInputDTO input, CancellationToken cancellationToken);
        Task<UserSiteOutputDTO> UpdateUserSite(UserSiteInputDTO input, CancellationToken cancellationToken);
        Task<UserSiteOutputDTO> GetUserSiteById(string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<UserSiteOutputDTO>> GetAllUserSites(UserSiteFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteUserSite(string siteId, CancellationToken cancellationToken);
        Task<List<UserSiteOutputDTO>> ImportUserSites(Stream stream);
        Task<string> ExportUserSites(CancellationToken cancellationToken);
    }
}
