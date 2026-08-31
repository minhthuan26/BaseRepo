using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Function.CreateFunction
{
    public class CreateFunctionCommandHandler : IRequestHandler<CreateFunctionCommand, FunctionOutputDTO>
    {
        private readonly IFunctionRepository _repository;

        public CreateFunctionCommandHandler(IFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<FunctionOutputDTO> Handle(CreateFunctionCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateFunction(request.Data, cancellationToken);
            return result;
        }
    }
}
