using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Module.GetModuleById
{
    public class GetModuleByIdQuery : IRequest<ModuleOutputDTO>
    {
        public string Data { get; set; }
    }
}
