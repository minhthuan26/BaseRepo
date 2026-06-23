using CommonLibs.DTOs;
using Site.Domain.DTOs;
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
        Task<UserRoleOutputDTO> UpdateUserRole(UserRoleInputDTO input, CancellationToken cancellationToken);
        Task<UserRoleOutputDTO> GetUserRoleById(string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<UserRoleOutputDTO>> GetAllUserRoles(UserRoleFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteUserRole(string siteId, CancellationToken cancellationToken);
        Task<List<UserRoleOutputDTO>> ImportUserRoles(Stream stream);
        Task<string> ExportUserRoles(CancellationToken cancellationToken);
    }
}
