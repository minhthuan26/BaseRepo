using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Function.ImportFunctions
{
    public class ImportFunctionsCommand : IRequest<List<FunctionOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
