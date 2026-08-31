using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Module.CreateModule
{
    public class CreateModuleCommand : IRequest<ModuleOutputDTO>
    {
        public ModuleInputDTO Data { get; set; }
    }
}
