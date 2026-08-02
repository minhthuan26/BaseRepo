using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Module.DeleteModule
{
    public class DeleteModuleCommand : IRequest<string>
    {
        public DeleteModuleInputDTO Data { get; set; }
    }
}
