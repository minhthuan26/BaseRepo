using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Module.GetModuleById
{
    public class GetModuleByIdQueryHandler : IRequestHandler<GetModuleByIdQuery, ModuleOutputDTO>
    {
        private readonly IModuleRepository _repository;

        public GetModuleByIdQueryHandler(IModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<ModuleOutputDTO> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetModuleById(request.Data, cancellationToken);
            return result;
        }
    }
}
