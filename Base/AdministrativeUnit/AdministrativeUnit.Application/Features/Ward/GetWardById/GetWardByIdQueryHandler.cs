using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.GetWardById
{
    public class GetWardByIdQueryHandler : IRequestHandler<GetWardByIdQuery, WardOutputDTO>
    {
        private readonly IWardRepository _repository;

        public GetWardByIdQueryHandler(IWardRepository repository)
        {
            _repository = repository;
        }

        public async Task<WardOutputDTO> Handle(GetWardByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetWardById(request.Data, cancellationToken);
            return result;
        }
    }
}
