using CommonLibs.DTOs;
using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.Function.GetAllFunctions
{
    public class GetAllFunctionsQueryHandler : IRequestHandler<GetAllFunctionsQuery, BaseFilterOutput<FunctionOutputDTO>>
    {
        private readonly IFunctionRepository _repository;

        public GetAllFunctionsQueryHandler(IFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<FunctionOutputDTO>> Handle(GetAllFunctionsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllFunctions(request.Filter, cancellationToken);
            return result;
        }
    }
}
