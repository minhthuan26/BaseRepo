using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.UpdateFieldType
{
    public class UpdateFieldTypeCommand : IRequest<FieldTypeOutputDTO>
    {
        public FieldTypeInputDTO Data { get; set; } = null!;
    }
}
