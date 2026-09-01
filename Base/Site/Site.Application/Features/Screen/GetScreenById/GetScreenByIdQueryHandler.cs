using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Screen.GetScreenById
{
    public class GetScreenByIdQueryHandler : IRequestHandler<GetScreenByIdQuery, ScreenOutputDTO>
    {
        private readonly IScreenRepository _repository;

        public GetScreenByIdQueryHandler(IScreenRepository repository)
        {
            _repository = repository;
        }

        public async Task<ScreenOutputDTO> Handle(GetScreenByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetScreenById(request.Data, cancellationToken);
            return result;
        }
    }
}
