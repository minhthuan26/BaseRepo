using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.Module.UpdateModule
{
    public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, ModuleOutputDTO>
    {
        private readonly IModuleRepository _repository;

        public UpdateModuleCommandHandler(IModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<ModuleOutputDTO> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.UpdateModule(request.Data, cancellationToken);
            return result;
        }
    }
}
