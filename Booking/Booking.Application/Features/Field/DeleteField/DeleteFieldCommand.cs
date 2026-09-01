using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.DeleteField
{
    public class DeleteFieldCommand : IRequest<string>
    {
        public DeleteFieldInputDTO Data { get; set; } = null!;
        public string CallerSiteId { get; set; } = null!;
    }
}
