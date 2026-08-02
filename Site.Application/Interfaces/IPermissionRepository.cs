using Site.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface IPermissionRepository
    {
        Task<PermissionTreeDTO> GetPermissionTree(string siteId, string? roleId, CancellationToken cancellationToken);
        Task<List<string>> AssignRolePermissions(string roleId, List<string> functionIds, CancellationToken cancellationToken);
    }
}
