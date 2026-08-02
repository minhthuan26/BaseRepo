using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserRole.CreateUserRole
{
    public class CreateUserRoleCommand : IRequest<UserRoleOutputDTO>
    {
        public UserRoleInputDTO Data { get; set; }
    }
}
