using MediatR;
using Booking.Application.Interfaces;

namespace Booking.Application.Features.Field.DeleteField
{
    public class DeleteFieldCommandHandler : IRequestHandler<DeleteFieldCommand, string>
    {
        private readonly IFieldRepository _repository;

        public DeleteFieldCommandHandler(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteFieldCommand request, CancellationToken cancellationToken)
        {
            return await _repository.DeleteField(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
