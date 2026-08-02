using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.RoleFunction.ImportRoleFunctions
{
    public class ImportRoleFunctionsCommand : IRequest<List<RoleFunctionOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
