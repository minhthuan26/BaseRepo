using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.ImportProvinces
{
    public class ImportProvincesCommand : IRequest<List<ProvinceOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
