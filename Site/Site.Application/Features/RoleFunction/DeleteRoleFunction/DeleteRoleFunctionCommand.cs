using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.RoleFunction.DeleteRoleFunction
{
    public class DeleteRoleFunctionCommand : IRequest<string>
    {
        public DeleteRoleFunctionInputDTO Data { get; set; }
    }
}
