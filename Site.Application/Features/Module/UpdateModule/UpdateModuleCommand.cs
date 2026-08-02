using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Module.UpdateModule
{
    public class UpdateModuleCommand : IRequest<ModuleOutputDTO>
    {
        public ModuleInputDTO Data { get; set; }
    }
}
