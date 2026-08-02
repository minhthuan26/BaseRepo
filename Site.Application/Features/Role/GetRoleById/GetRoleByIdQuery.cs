using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Role.GetRoleById
{
    public class GetRoleByIdQuery : IRequest<RoleOutputDTO>
    {
        public string Data { get; set; }
    }
}
