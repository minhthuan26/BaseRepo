using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Role.DeleteRole
{
    public class DeleteRoleCommand : IRequest<string>
    {
        public DeleteRoleInputDTO Data { get; set; }
    }
}
