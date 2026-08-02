using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Domain.DTOs;
using CommonLibs.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.GetAllWards
{
    public class GetAllWardsQueryHandler : IRequestHandler<GetAllWardsQuery, BaseFilterOutput<WardOutputDTO>>
    {
        private readonly IWardRepository _repository;

        public GetAllWardsQueryHandler(IWardRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<WardOutputDTO>> Handle(GetAllWardsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllWards(request.Filter, cancellationToken);
            return result;
        }
    }
}
