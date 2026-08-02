using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.Function.UpdateFunction
{
    public class UpdateFunctionCommandHandler : IRequestHandler<UpdateFunctionCommand, FunctionOutputDTO>
    {
        private readonly IFunctionRepository _repository;

        public UpdateFunctionCommandHandler(IFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<FunctionOutputDTO> Handle(UpdateFunctionCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.UpdateFunction(request.Data, cancellationToken);
            return result;
        }
    }
}
