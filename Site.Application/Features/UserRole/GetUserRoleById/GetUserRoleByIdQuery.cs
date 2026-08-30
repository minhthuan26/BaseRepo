using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserRole.GetUserRoleById
{
    public class GetUserRoleByIdQuery : IRequest<UserRoleOutputDTO>
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string SiteId { get; set; }
    }
}
