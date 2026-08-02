using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Screen.DeleteScreen
{
    public class DeleteScreenCommandHandler : IRequestHandler<DeleteScreenCommand, string>
    {
        private readonly IScreenRepository _repository;

        public DeleteScreenCommandHandler(IScreenRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteScreenCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteScreen(request.Data, cancellationToken);
            return result;
        }
    }
}
