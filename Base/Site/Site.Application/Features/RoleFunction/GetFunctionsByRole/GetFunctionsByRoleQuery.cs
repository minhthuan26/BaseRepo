using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.RoleFunction.GetFunctionsByRole
{
    public class GetFunctionsByRoleQuery : IRequest<List<FunctionOutputDTO>>
    {
        public string RoleId { get; set; }
        public string SiteId { get; set; }
    }
}
