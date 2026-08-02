using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.RoleFunction.ExportRoleFunctions
{
    public class ExportRoleFunctionsQueryHandler : IRequestHandler<ExportRoleFunctionsQuery, string>
    {
        private readonly IRoleFunctionRepository _repository;

        public ExportRoleFunctionsQueryHandler(IRoleFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportRoleFunctionsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportRoleFunctions(cancellationToken);
            return result;
        }
    }
}
