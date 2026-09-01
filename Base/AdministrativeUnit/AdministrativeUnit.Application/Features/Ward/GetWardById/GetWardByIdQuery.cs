using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.GetWardById
{
    public class GetWardByIdQuery : IRequest<WardOutputDTO>
    {
        public string Data { get; set; }
    }
}
