using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.RoleFunction.DeleteRoleFunction
{
    public class DeleteRoleFunctionCommand : IRequest<string>
    {
        public DeleteRoleFunctionInputDTO Data { get; set; }
    }
}
