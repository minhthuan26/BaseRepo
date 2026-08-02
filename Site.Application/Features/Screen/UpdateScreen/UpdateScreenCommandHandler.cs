using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.Screen.UpdateScreen
{
    public class UpdateScreenCommandHandler : IRequestHandler<UpdateScreenCommand, ScreenOutputDTO>
    {
        private readonly IScreenRepository _repository;

        public UpdateScreenCommandHandler(IScreenRepository repository)
        {
            _repository = repository;
        }

        public async Task<ScreenOutputDTO> Handle(UpdateScreenCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.UpdateScreen(request.Data, cancellationToken);
            return result;
        }
    }
}
