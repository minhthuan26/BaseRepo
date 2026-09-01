using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.GetProvinceById
{
    public class GetProvinceByIdQuery : IRequest<ProvinceOutputDTO>
    {
        public string Data { get; set; }
    }
}
