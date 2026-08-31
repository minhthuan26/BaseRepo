using CommonLibs.DTOs;
using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Module.GetAllModules
{
    public class GetAllModulesQueryHandler : IRequestHandler<GetAllModulesQuery, BaseFilterOutput<ModuleOutputDTO>>
    {
        private readonly IModuleRepository _repository;

        public GetAllModulesQueryHandler(IModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<ModuleOutputDTO>> Handle(GetAllModulesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllModules(request.Filter, cancellationToken);
            return result;
        }
    }
}
