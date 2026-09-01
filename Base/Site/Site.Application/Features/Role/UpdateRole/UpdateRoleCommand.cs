using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Role.UpdateRole
{
    public class UpdateRoleCommand : IRequest<RoleOutputDTO>
    {
        public RoleInputDTO Data { get; set; }
    }
}
