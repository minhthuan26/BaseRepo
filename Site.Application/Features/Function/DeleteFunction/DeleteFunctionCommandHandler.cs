using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Function.DeleteFunction
{
    public class DeleteFunctionCommandHandler : IRequestHandler<DeleteFunctionCommand, string>
    {
        private readonly IFunctionRepository _repository;

        public DeleteFunctionCommandHandler(IFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteFunctionCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteFunction(request.Data, cancellationToken);
            return result;
        }
    }
}
