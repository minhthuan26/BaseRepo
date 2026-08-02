using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserRole.GetRolesByUser
{
    public class GetRolesByUserQuery : IRequest<List<RoleOutputDTO>>
    {
        public string UserId { get; set; }
    }
}
