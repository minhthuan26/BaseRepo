using MediatR;
using Booking.Application.Interfaces;

namespace Booking.Application.Features.FieldType.DeleteFieldType
{
    public class DeleteFieldTypeCommandHandler : IRequestHandler<DeleteFieldTypeCommand, string>
    {
        private readonly IFieldTypeRepository _repository;

        public DeleteFieldTypeCommandHandler(IFieldTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteFieldTypeCommand request, CancellationToken cancellationToken)
        {
            return await _repository.DeleteFieldType(request.Data, cancellationToken);
        }
    }
}
