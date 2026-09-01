using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.GetFieldTypeById
{
    public class GetFieldTypeByIdQuery : IRequest<FieldTypeOutputDTO>
    {
        public string Data { get; set; } = null!;
    }
}
