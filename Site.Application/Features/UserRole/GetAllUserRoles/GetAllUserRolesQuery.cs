using CommonLibs.DTOs;
using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserRole.GetAllUserRoles
{
    public class GetAllUserRolesQuery : IRequest<BaseFilterOutput<UserRoleOutputDTO>>
    {
        public UserRoleFilterDTO Filter { get; set; }
    }
}
