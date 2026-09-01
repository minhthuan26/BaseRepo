using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Contract.DTOs;
using CommonLibs.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.GetAllProvinces
{
    public class GetAllProvincesQueryHandler : IRequestHandler<GetAllProvincesQuery, BaseFilterOutput<ProvinceOutputDTO>>
    {
        private readonly IProvinceRepository _repository;

        public GetAllProvincesQueryHandler(IProvinceRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<ProvinceOutputDTO>> Handle(GetAllProvincesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllProvinces(request.Filter, cancellationToken);
            return result;
        }
    }
}
