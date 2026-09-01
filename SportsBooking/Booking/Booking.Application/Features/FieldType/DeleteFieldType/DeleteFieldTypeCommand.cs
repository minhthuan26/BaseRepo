using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.DeleteFieldType
{
    public class DeleteFieldTypeCommand : IRequest<string>
    {
        public DeleteFieldTypeInputDTO Data { get; set; } = null!;
    }
}
