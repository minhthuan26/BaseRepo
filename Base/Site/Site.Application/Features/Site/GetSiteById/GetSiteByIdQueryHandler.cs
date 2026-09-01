using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Site.GetSiteById
{
    public class GetSiteByIdQueryHandler : IRequestHandler<GetSiteByIdQuery, SiteOutputDTO>
    {
        private readonly ISiteRepository _repository;

        public GetSiteByIdQueryHandler(ISiteRepository repository)
        {
            _repository = repository;
        }

        public async Task<SiteOutputDTO> Handle(GetSiteByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetSiteById(request.Data, cancellationToken);
            return result;
        }
    }
}
