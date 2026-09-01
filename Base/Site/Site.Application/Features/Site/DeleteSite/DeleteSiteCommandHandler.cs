using MediatR;
using Site.Application.Interfaces;

namespace Site.Application.Features.Site.DeleteSite
{
    public class DeleteSiteCommandHandler : IRequestHandler<DeleteSiteCommand, string>
    {
        private readonly ISiteRepository _repository;

        public DeleteSiteCommandHandler(ISiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteSiteCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteSite(request.Data, cancellationToken);
            return result;
        }
    }
}
