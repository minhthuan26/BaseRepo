using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Role.UpdateRole
{
    public class UpdateRoleCommand : IRequest<RoleOutputDTO>
    {
        public RoleInputDTO Data { get; set; }
    }
}
