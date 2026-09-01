using CommonLibs.DTOs;
using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Role.GetAllRoles
{
    public class GetAllRolesQuery : IRequest<BaseFilterOutput<RoleOutputDTO>>
    {
        public RoleFilterDTO Filter { get; set; }
    }
}
