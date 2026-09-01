using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.DeleteWard
{
    public class DeleteWardCommand : IRequest<string>
    {
        public DeleteWardInputDTO Data { get; set; }
    }
}
