using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserRole.DeleteUserRole
{
    public class DeleteUserRoleCommand : IRequest<string>
    {
        public DeleteUserRoleInputDTO Data { get; set; }
    }
}
