using CommonLibs.DTOs;
using Site.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface IRoleFunctionRepository
    {
        Task<RoleFunctionOutputDTO> CreateRoleFunction(RoleFunctionInputDTO input, CancellationToken cancellationToken);
        Task<RoleFunctionOutputDTO> UpdateRoleFunction(RoleFunctionInputDTO input, CancellationToken cancellationToken);
        Task<RoleFunctionOutputDTO> GetRoleFunctionById(string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<RoleFunctionOutputDTO>> GetAllRoleFunctions(RoleFunctionFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteRoleFunction(string siteId, CancellationToken cancellationToken);
        Task<List<RoleFunctionOutputDTO>> ImportRoleFunctions(Stream stream);
        Task<string> ExportRoleFunctions(CancellationToken cancellationToken);
    }
}
