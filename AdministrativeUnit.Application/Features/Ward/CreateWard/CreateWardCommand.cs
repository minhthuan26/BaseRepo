using AdministrativeUnit.Domain.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.CreateWard
{
    public class CreateWardCommand : IRequest<WardOutputDTO>
    {
        public WardInputDTO Data { get; set; }
    }
}
