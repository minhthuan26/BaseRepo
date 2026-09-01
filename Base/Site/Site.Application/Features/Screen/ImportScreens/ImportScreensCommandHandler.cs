using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Screen.ImportScreens
{
    public class ImportScreensCommandHandler : IRequestHandler<ImportScreensCommand, List<ScreenOutputDTO>>
    {
        private readonly IScreenRepository _repository;

        public ImportScreensCommandHandler(IScreenRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ScreenOutputDTO>> Handle(ImportScreensCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportScreens(request.Data, request.FileName, cancellationToken);
            return result;
        }
    }
}
