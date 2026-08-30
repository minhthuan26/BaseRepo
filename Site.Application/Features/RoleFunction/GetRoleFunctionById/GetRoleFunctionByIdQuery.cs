using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.RoleFunction.GetRoleFunctionById
{
    public class GetRoleFunctionByIdQuery : IRequest<RoleFunctionOutputDTO>
    {
        public string RoleId { get; set; }
        public string FunctionId { get; set; }
        public string SiteId { get; set; }
    }
}
