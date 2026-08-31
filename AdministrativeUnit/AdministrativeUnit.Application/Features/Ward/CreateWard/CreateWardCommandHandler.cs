using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.CreateWard
{
    public class CreateWardCommandHandler : IRequestHandler<CreateWardCommand, WardOutputDTO>
    {
        private readonly IWardRepository _repository;

        public CreateWardCommandHandler(IWardRepository repository)
        {
            _repository = repository;
        }

        public async Task<WardOutputDTO> Handle(CreateWardCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateWard(request.Data, cancellationToken);
            return result;
        }
    }
}
