using CommonLibs.DTOs;
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.GetAllFieldTypes
{
    public class GetAllFieldTypesQueryHandler : IRequestHandler<GetAllFieldTypesQuery, BaseFilterOutput<FieldTypeOutputDTO>>
    {
        private readonly IFieldTypeRepository _repository;

        public GetAllFieldTypesQueryHandler(IFieldTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<FieldTypeOutputDTO>> Handle(GetAllFieldTypesQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllFieldTypes(request.Filter, cancellationToken);
        }
    }
}
