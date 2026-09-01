using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.GetFieldById
{
    public class GetFieldByIdQuery : IRequest<FieldOutputDTO>
    {
        public string Data { get; set; } = null!;
    }
}
