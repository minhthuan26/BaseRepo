using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.CreateField
{
    public class CreateFieldCommandHandler : IRequestHandler<CreateFieldCommand, FieldOutputDTO>
    {
        private readonly IFieldRepository _repository;

        public CreateFieldCommandHandler(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldOutputDTO> Handle(CreateFieldCommand request, CancellationToken cancellationToken)
        {
            return await _repository.CreateField(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
