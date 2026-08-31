using CommonLibs.DTOs;
using Site.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<UserRoleOutputDTO> CreateUserRole(UserRoleInputDTO input, CancellationToken cancellationToken);
        Task<UserRoleOutputDTO> GetUserRoleById(string userId, string roleId, string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<UserRoleOutputDTO>> GetAllUserRoles(UserRoleFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteUserRole(DeleteUserRoleInputDTO input, CancellationToken cancellationToken);
        Task<List<UserRoleOutputDTO>> ImportUserRoles(Stream stream, string fileName, string siteId, CancellationToken cancellationToken);
        Task<string> ExportUserRoles(string siteId, CancellationToken cancellationToken);
        Task<List<RoleOutputDTO>> GetRolesByUser(string userId, string siteId, CancellationToken cancellationToken);
    }
}
