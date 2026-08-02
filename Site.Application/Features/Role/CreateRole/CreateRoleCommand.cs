using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Role.CreateRole
{
    public class CreateRoleCommand : IRequest<RoleOutputDTO>
    {
        public RoleInputDTO Data { get; set; }
    }
}
