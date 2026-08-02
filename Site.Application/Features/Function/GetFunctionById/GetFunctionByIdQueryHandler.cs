using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.Function.GetFunctionById
{
    public class GetFunctionByIdQueryHandler : IRequestHandler<GetFunctionByIdQuery, FunctionOutputDTO>
    {
        private readonly IFunctionRepository _repository;

        public GetFunctionByIdQueryHandler(IFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<FunctionOutputDTO> Handle(GetFunctionByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetFunctionById(request.Data, cancellationToken);
            return result;
        }
    }
}
