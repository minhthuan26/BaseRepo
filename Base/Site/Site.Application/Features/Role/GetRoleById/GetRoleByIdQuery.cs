using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Role.GetRoleById
{
    public class GetRoleByIdQuery : IRequest<RoleOutputDTO>
    {
        public string Data { get; set; }
        public string SiteId { get; set; }
    }
}
