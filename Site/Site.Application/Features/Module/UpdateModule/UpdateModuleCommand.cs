using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Module.UpdateModule
{
    public class UpdateModuleCommand : IRequest<ModuleOutputDTO>
    {
        public ModuleInputDTO Data { get; set; }
    }
}
