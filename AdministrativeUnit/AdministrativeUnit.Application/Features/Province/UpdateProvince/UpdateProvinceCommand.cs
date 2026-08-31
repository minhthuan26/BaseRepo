using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.UpdateProvince
{
    public class UpdateProvinceCommand : IRequest<ProvinceOutputDTO>
    {
        public ProvinceInputDTO Data { get; set; }
    }
}
