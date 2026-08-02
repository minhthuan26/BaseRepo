using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Domain.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.UpdateProvince
{
    public class UpdateProvinceCommandHandler : IRequestHandler<UpdateProvinceCommand, ProvinceOutputDTO>
    {
        private readonly IProvinceRepository _repository;

        public UpdateProvinceCommandHandler(IProvinceRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProvinceOutputDTO> Handle(UpdateProvinceCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.UpdateProvince(request.Data, cancellationToken);
            return result;
        }
    }
}
