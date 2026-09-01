using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.CreateField
{
    public class CreateFieldCommand : IRequest<FieldOutputDTO>
    {
        public FieldInputDTO Data { get; set; } = null!;
    }
}
