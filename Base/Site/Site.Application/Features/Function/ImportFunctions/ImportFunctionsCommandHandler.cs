using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Function.ImportFunctions
{
    public class ImportFunctionsCommandHandler : IRequestHandler<ImportFunctionsCommand, List<FunctionOutputDTO>>
    {
        private readonly IFunctionRepository _repository;

        public ImportFunctionsCommandHandler(IFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FunctionOutputDTO>> Handle(ImportFunctionsCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportFunctions(request.Data, request.FileName, cancellationToken);
            return result;
        }
    }
}
