using CommonLibs.DTOs;
using Site.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface IFunctionRepository
    {
        Task<FunctionOutputDTO> CreateFunction(FunctionInputDTO input, CancellationToken cancellationToken);
        Task<FunctionOutputDTO> UpdateFunction(FunctionInputDTO input, CancellationToken cancellationToken);
        Task<FunctionOutputDTO> GetFunctionById(string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<FunctionOutputDTO>> GetAllFunctions(FunctionFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteFunction(string siteId, CancellationToken cancellationToken);
        Task<List<FunctionOutputDTO>> ImportFunctions(Stream stream);
        Task<string> ExportFunctions(CancellationToken cancellationToken);
    }
}
