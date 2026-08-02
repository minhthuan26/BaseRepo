using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Module.CreateModule
{
    public class CreateModuleCommand : IRequest<ModuleOutputDTO>
    {
        public ModuleInputDTO Data { get; set; }
    }
}
