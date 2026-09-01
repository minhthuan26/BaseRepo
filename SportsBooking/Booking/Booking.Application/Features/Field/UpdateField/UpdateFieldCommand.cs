using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.UpdateField
{
    public class UpdateFieldCommand : IRequest<FieldOutputDTO>
    {
        public FieldInputDTO Data { get; set; } = null!;
        public string CallerSiteId { get; set; } = null!;
    }
}
