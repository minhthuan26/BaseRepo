using CommonLibs.DTOs;
using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.RoleFunction.GetAllRoleFunctions
{
    public class GetAllRoleFunctionsQuery : IRequest<BaseFilterOutput<RoleFunctionOutputDTO>>
    {
        public RoleFunctionFilterDTO Filter { get; set; }
    }
}
