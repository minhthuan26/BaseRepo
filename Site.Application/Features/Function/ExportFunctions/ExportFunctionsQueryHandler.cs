using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Function.ExportFunctions
{
    public class ExportFunctionsQueryHandler : IRequestHandler<ExportFunctionsQuery, string>
    {
        private readonly IFunctionRepository _repository;

        public ExportFunctionsQueryHandler(IFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportFunctionsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportFunctions(cancellationToken);
            return result;
        }
    }
}
