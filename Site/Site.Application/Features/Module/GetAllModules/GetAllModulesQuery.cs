using CommonLibs.DTOs;
using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Module.GetAllModules
{
    public class GetAllModulesQuery : IRequest<BaseFilterOutput<ModuleOutputDTO>>
    {
        public ModuleFilterDTO Filter { get; set; }
    }
}
