using CommonLibs.DTOs;
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.GetAllFields
{
    public class GetAllFieldsQueryHandler : IRequestHandler<GetAllFieldsQuery, BaseFilterOutput<FieldOutputDTO>>
    {
        private readonly IFieldRepository _repository;

        public GetAllFieldsQueryHandler(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<FieldOutputDTO>> Handle(GetAllFieldsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllFields(request.Filter, cancellationToken);
        }
    }
}
