using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Domain.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.UpdateWard
{
    public class UpdateWardCommandHandler : IRequestHandler<UpdateWardCommand, WardOutputDTO>
    {
        private readonly IWardRepository _repository;

        public UpdateWardCommandHandler(IWardRepository repository)
        {
            _repository = repository;
        }

        public async Task<WardOutputDTO> Handle(UpdateWardCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.UpdateWard(request.Data, cancellationToken);
            return result;
        }
    }
}
