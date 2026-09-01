using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.GetProvinceById
{
    public class GetProvinceByIdQueryHandler : IRequestHandler<GetProvinceByIdQuery, ProvinceOutputDTO>
    {
        private readonly IProvinceRepository _repository;

        public GetProvinceByIdQueryHandler(IProvinceRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProvinceOutputDTO> Handle(GetProvinceByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetProvinceById(request.Data, cancellationToken);
            return result;
        }
    }
}
