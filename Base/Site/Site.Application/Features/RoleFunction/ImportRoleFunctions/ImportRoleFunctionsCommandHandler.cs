using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.RoleFunction.ImportRoleFunctions
{
    public class ImportRoleFunctionsCommandHandler : IRequestHandler<ImportRoleFunctionsCommand, List<RoleFunctionOutputDTO>>
    {
        private readonly IRoleFunctionRepository _repository;

        public ImportRoleFunctionsCommandHandler(IRoleFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RoleFunctionOutputDTO>> Handle(ImportRoleFunctionsCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportRoleFunctions(request.Data, request.FileName, request.SiteId, cancellationToken);
            return result;
        }
    }
}
