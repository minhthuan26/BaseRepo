using AdministrativeUnit.Contract.DTOs;
using CommonLibs.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.GetAllProvinces
{
    public class GetAllProvincesQuery : IRequest<BaseFilterOutput<ProvinceOutputDTO>>
    {
        public ProvinceFilterDTO Filter { get; set; }
    }
}
