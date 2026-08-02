using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Role.DeleteRole
{
    public class DeleteRoleCommand : IRequest<string>
    {
        public DeleteRoleInputDTO Data { get; set; }
    }
}
