using AdministrativeUnit.Domain.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.GetWardsByProvince
{
    public class GetWardsByProvinceQuery : IRequest<List<WardOutputDTO>>
    {
        public string ProvinceCode { get; set; }
    }
}
