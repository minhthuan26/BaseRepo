using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Domain.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.CreateProvince
{
    public class CreateProvinceCommandHandler : IRequestHandler<CreateProvinceCommand, ProvinceOutputDTO>
    {
        private readonly IProvinceRepository _repository;

        public CreateProvinceCommandHandler(IProvinceRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProvinceOutputDTO> Handle(CreateProvinceCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateProvince(request.Data, cancellationToken);
            return result;
        }
    }
}
