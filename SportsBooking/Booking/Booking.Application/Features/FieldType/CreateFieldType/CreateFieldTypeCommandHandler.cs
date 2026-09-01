using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.CreateFieldType
{
    public class CreateFieldTypeCommandHandler : IRequestHandler<CreateFieldTypeCommand, FieldTypeOutputDTO>
    {
        private readonly IFieldTypeRepository _repository;

        public CreateFieldTypeCommandHandler(IFieldTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldTypeOutputDTO> Handle(CreateFieldTypeCommand request, CancellationToken cancellationToken)
        {
            return await _repository.CreateFieldType(request.Data, cancellationToken);
        }
    }
}
