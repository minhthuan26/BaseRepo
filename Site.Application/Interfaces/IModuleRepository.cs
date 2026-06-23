using CommonLibs.DTOs;
using Site.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface IModuleRepository
    {
        Task<ModuleOutputDTO> CreateModule(ModuleInputDTO input, CancellationToken cancellationToken);
        Task<ModuleOutputDTO> UpdateModule(ModuleInputDTO input, CancellationToken cancellationToken);
        Task<ModuleOutputDTO> GetModuleById(string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<ModuleOutputDTO>> GetAllModules(ModuleFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteModule(string siteId, CancellationToken cancellationToken);
        Task<List<ModuleOutputDTO>> ImportModules(Stream stream);
        Task<string> ExportModules(CancellationToken cancellationToken);
    }
}
