using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.GetFieldById
{
    public class GetFieldByIdQueryHandler : IRequestHandler<GetFieldByIdQuery, FieldOutputDTO>
    {
        private readonly IFieldRepository _repository;

        public GetFieldByIdQueryHandler(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldOutputDTO> Handle(GetFieldByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetFieldById(request.Data, cancellationToken);
        }
    }
}
