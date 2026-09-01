using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.UpdateWard
{
    public class UpdateWardCommand : IRequest<WardOutputDTO>
    {
        public WardInputDTO Data { get; set; }
    }
}
