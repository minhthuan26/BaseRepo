using AdministrativeUnit.Domain.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.DeleteProvince
{
    public class DeleteProvinceCommand : IRequest<string>
    {
        public DeleteProvinceInputDTO Data { get; set; }
    }
}
