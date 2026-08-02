using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.RoleFunction.CreateRoleFunction
{
    public class CreateRoleFunctionCommandHandler : IRequestHandler<CreateRoleFunctionCommand, RoleFunctionOutputDTO>
    {
        private readonly IRoleFunctionRepository _repository;

        public CreateRoleFunctionCommandHandler(IRoleFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<RoleFunctionOutputDTO> Handle(CreateRoleFunctionCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateRoleFunction(request.Data, cancellationToken);
            return result;
        }
    }
}
