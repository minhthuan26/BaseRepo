using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.UpdateField
{
    public class UpdateFieldCommandHandler : IRequestHandler<UpdateFieldCommand, FieldOutputDTO>
    {
        private readonly IFieldRepository _repository;

        public UpdateFieldCommandHandler(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldOutputDTO> Handle(UpdateFieldCommand request, CancellationToken cancellationToken)
        {
            return await _repository.UpdateField(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
