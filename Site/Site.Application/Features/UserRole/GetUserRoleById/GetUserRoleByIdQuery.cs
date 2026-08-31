using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.UserRole.GetUserRoleById
{
    public class GetUserRoleByIdQuery : IRequest<UserRoleOutputDTO>
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string SiteId { get; set; }
    }
}
