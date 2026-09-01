using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.CreateFieldType
{
    public class CreateFieldTypeCommand : IRequest<FieldTypeOutputDTO>
    {
        public FieldTypeInputDTO Data { get; set; } = null!;
    }
}
