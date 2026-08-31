using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.GetWardsByProvince
{
    public class GetWardsByProvinceQueryHandler : IRequestHandler<GetWardsByProvinceQuery, List<WardOutputDTO>>
    {
        private readonly IWardRepository _repository;

        public GetWardsByProvinceQueryHandler(IWardRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<WardOutputDTO>> Handle(GetWardsByProvinceQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetWardsByProvince(request.ProvinceCode, cancellationToken);
            return result;
        }
    }
}
