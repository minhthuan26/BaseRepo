using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Module.ExportModules
{
    public class ExportModulesQueryHandler : IRequestHandler<ExportModulesQuery, string>
    {
        private readonly IModuleRepository _repository;

        public ExportModulesQueryHandler(IModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportModulesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportModules(cancellationToken);
            return result;
        }
    }
}
