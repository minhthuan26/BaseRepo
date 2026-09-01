using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.UpdateFieldType
{
    public class UpdateFieldTypeCommandHandler : IRequestHandler<UpdateFieldTypeCommand, FieldTypeOutputDTO>
    {
        private readonly IFieldTypeRepository _repository;

        public UpdateFieldTypeCommandHandler(IFieldTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldTypeOutputDTO> Handle(UpdateFieldTypeCommand request, CancellationToken cancellationToken)
        {
            return await _repository.UpdateFieldType(request.Data, cancellationToken);
        }
    }
}
