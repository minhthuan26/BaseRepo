using CommonLibs.DTOs;
using Site.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<RoleOutputDTO> CreateRole(RoleInputDTO input, CancellationToken cancellationToken);
        Task<RoleOutputDTO> UpdateRole(RoleInputDTO input, CancellationToken cancellationToken);
        Task<RoleOutputDTO> GetRoleById(string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<RoleOutputDTO>> GetAllRoles(RoleFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteRole(string siteId, CancellationToken cancellationToken);
        Task<List<RoleOutputDTO>> ImportRoles(Stream stream);
        Task<string> ExportRoles(CancellationToken cancellationToken);
    }
}
