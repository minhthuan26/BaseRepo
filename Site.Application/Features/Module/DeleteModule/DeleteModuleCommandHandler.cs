using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Module.DeleteModule
{
    public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand, string>
    {
        private readonly IModuleRepository _repository;

        public DeleteModuleCommandHandler(IModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteModule(request.Data, cancellationToken);
            return result;
        }
    }
}
