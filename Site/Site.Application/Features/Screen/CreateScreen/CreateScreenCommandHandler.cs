using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Screen.CreateScreen
{
    public class CreateScreenCommandHandler : IRequestHandler<CreateScreenCommand, ScreenOutputDTO>
    {
        private readonly IScreenRepository _repository;

        public CreateScreenCommandHandler(IScreenRepository repository)
        {
            _repository = repository;
        }

        public async Task<ScreenOutputDTO> Handle(CreateScreenCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateScreen(request.Data, cancellationToken);
            return result;
        }
    }
}
