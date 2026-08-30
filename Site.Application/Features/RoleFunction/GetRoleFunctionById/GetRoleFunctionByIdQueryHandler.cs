using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.RoleFunction.GetRoleFunctionById
{
    public class GetRoleFunctionByIdQueryHandler : IRequestHandler<GetRoleFunctionByIdQuery, RoleFunctionOutputDTO>
    {
        private readonly IRoleFunctionRepository _repository;

        public GetRoleFunctionByIdQueryHandler(IRoleFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<RoleFunctionOutputDTO> Handle(GetRoleFunctionByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetRoleFunctionById(request.RoleId, request.FunctionId, request.SiteId, cancellationToken);
            return result;
        }
    }
}
