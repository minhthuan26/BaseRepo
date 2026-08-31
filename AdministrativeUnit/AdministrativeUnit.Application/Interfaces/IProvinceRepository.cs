using AdministrativeUnit.Contract.DTOs;
using CommonLibs.DTOs;

namespace AdministrativeUnit.Application.Interfaces
{
    public interface IProvinceRepository
    {
        Task<ProvinceOutputDTO> CreateProvince(ProvinceInputDTO input, CancellationToken cancellationToken);
        Task<ProvinceOutputDTO> UpdateProvince(ProvinceInputDTO input, CancellationToken cancellationToken);
        Task<ProvinceOutputDTO> GetProvinceById(string code, CancellationToken cancellationToken);
        Task<BaseFilterOutput<ProvinceOutputDTO>> GetAllProvinces(ProvinceFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteProvince(DeleteProvinceInputDTO input, CancellationToken cancellationToken);
        Task<List<ProvinceOutputDTO>> ImportProvinces(Stream stream, string fileName, CancellationToken cancellationToken);
        Task<string> ExportProvinces(CancellationToken cancellationToken);
    }
}
