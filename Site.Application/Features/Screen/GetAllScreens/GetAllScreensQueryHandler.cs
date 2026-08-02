using CommonLibs.DTOs;
using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.Screen.GetAllScreens
{
    public class GetAllScreensQueryHandler : IRequestHandler<GetAllScreensQuery, BaseFilterOutput<ScreenOutputDTO>>
    {
        private readonly IScreenRepository _repository;

        public GetAllScreensQueryHandler(IScreenRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<ScreenOutputDTO>> Handle(GetAllScreensQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllScreens(request.Filter, cancellationToken);
            return result;
        }
    }
}
