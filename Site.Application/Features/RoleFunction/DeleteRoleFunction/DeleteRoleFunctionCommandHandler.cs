using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.RoleFunction.DeleteRoleFunction
{
    public class DeleteRoleFunctionCommandHandler : IRequestHandler<DeleteRoleFunctionCommand, string>
    {
        private readonly IRoleFunctionRepository _repository;

        public DeleteRoleFunctionCommandHandler(IRoleFunctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteRoleFunctionCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteRoleFunction(request.Data, cancellationToken);
            return result;
        }
    }
}
