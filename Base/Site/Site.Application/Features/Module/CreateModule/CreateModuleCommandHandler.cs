using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Module.CreateModule
{
    public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, ModuleOutputDTO>
    {
        private readonly IModuleRepository _repository;

        public CreateModuleCommandHandler(IModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<ModuleOutputDTO> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateModule(request.Data, cancellationToken);
            return result;
        }
    }
}
