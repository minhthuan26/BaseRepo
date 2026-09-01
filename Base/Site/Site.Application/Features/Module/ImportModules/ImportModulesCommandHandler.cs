using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Module.ImportModules
{
    public class ImportModulesCommandHandler : IRequestHandler<ImportModulesCommand, List<ModuleOutputDTO>>
    {
        private readonly IModuleRepository _repository;

        public ImportModulesCommandHandler(IModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ModuleOutputDTO>> Handle(ImportModulesCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportModules(request.Data, request.FileName, cancellationToken);
            return result;
        }
    }
}
