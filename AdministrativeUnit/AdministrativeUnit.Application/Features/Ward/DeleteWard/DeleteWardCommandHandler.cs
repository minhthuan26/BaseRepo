using AdministrativeUnit.Application.Interfaces;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.DeleteWard
{
    public class DeleteWardCommandHandler : IRequestHandler<DeleteWardCommand, string>
    {
        private readonly IWardRepository _repository;

        public DeleteWardCommandHandler(IWardRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteWardCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteWard(request.Data, cancellationToken);
            return result;
        }
    }
}
