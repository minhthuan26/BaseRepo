using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.RoleFunction.CreateRoleFunction
{
    public class CreateRoleFunctionCommand : IRequest<RoleFunctionOutputDTO>
    {
        public RoleFunctionInputDTO Data { get; set; }
    }
}
