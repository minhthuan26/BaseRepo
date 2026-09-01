using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.GetFieldTypeById
{
    public class GetFieldTypeByIdQueryHandler : IRequestHandler<GetFieldTypeByIdQuery, FieldTypeOutputDTO>
    {
        private readonly IFieldTypeRepository _repository;

        public GetFieldTypeByIdQueryHandler(IFieldTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldTypeOutputDTO> Handle(GetFieldTypeByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetFieldTypeById(request.Data, cancellationToken);
        }
    }
}
