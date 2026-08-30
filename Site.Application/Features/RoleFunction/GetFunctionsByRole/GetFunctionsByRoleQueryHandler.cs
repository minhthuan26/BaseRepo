using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.RoleFunction.GetFunctionsByRole
{
    public class GetFunctionsByRoleQueryHandler : IRequestHandler<GetFunctionsByRoleQuery, List<FunctionOutputDTO>>
    {
        private readonly IRoleFunctionRepository _repository;

        public GetFunctionsByRoleQueryHandler(IRoleFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FunctionOutputDTO>> Handle(GetFunctionsByRoleQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetFunctionsByRole(request.RoleId, request.SiteId, cancellationToken);
            return result;
        }
    }
}
