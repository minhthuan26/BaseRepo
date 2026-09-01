using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Module.ImportModules
{
    public class ImportModulesCommand : IRequest<List<ModuleOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
