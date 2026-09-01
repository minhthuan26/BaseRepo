using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.UserRole.DeleteUserRole
{
    public class DeleteUserRoleCommand : IRequest<string>
    {
        public DeleteUserRoleInputDTO Data { get; set; }
    }
}
