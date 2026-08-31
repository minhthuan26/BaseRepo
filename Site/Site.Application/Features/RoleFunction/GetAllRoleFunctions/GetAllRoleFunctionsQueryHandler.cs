using CommonLibs.DTOs;
using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.RoleFunction.GetAllRoleFunctions
{
    public class GetAllRoleFunctionsQueryHandler : IRequestHandler<GetAllRoleFunctionsQuery, BaseFilterOutput<RoleFunctionOutputDTO>>
    {
        private readonly IRoleFunctionRepository _repository;

        public GetAllRoleFunctionsQueryHandler(IRoleFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<RoleFunctionOutputDTO>> Handle(GetAllRoleFunctionsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllRoleFunctions(request.Filter, cancellationToken);
            return result;
        }
    }
}
