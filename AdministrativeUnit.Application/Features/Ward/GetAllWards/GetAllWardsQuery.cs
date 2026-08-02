using AdministrativeUnit.Domain.DTOs;
using CommonLibs.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.GetAllWards
{
    public class GetAllWardsQuery : IRequest<BaseFilterOutput<WardOutputDTO>>
    {
        public WardFilterDTO Filter { get; set; }
    }
}
