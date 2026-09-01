using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Screen.ExportScreens
{
    public class ExportScreensQueryHandler : IRequestHandler<ExportScreensQuery, string>
    {
        private readonly IScreenRepository _repository;

        public ExportScreensQueryHandler(IScreenRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportScreensQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportScreens(cancellationToken);
            return result;
        }
    }
}
