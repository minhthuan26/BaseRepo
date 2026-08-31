using AdministrativeUnit.Contract.DTOs;
using CommonLibs.DTOs;

namespace AdministrativeUnit.Application.Interfaces
{
    public interface IWardRepository
    {
        Task<WardOutputDTO> CreateWard(WardInputDTO input, CancellationToken cancellationToken);
        Task<WardOutputDTO> UpdateWard(WardInputDTO input, CancellationToken cancellationToken);
        Task<WardOutputDTO> GetWardById(string code, CancellationToken cancellationToken);
        Task<BaseFilterOutput<WardOutputDTO>> GetAllWards(WardFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteWard(DeleteWardInputDTO input, CancellationToken cancellationToken);
        Task<List<WardOutputDTO>> ImportWards(Stream stream, string fileName, CancellationToken cancellationToken);
        Task<string> ExportWards(CancellationToken cancellationToken);
        Task<List<WardOutputDTO>> GetWardsByProvince(string provinceCode, CancellationToken cancellationToken);
    }
}
