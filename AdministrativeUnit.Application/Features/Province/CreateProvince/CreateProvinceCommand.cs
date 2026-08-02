using AdministrativeUnit.Domain.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.CreateProvince
{
    public class CreateProvinceCommand : IRequest<ProvinceOutputDTO>
    {
        public ProvinceInputDTO Data { get; set; }
    }
}
