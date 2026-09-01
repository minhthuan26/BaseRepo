using CommonLibs.DTOs;
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.GetAllFields
{
    public class GetAllFieldsQuery : IRequest<BaseFilterOutput<FieldOutputDTO>>
    {
        public FieldFilterDTO Filter { get; set; } = null!;
    }
}
