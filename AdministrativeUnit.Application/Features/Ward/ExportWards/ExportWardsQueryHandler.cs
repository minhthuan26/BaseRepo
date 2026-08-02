using AdministrativeUnit.Application.Interfaces;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.ExportWards
{
    public class ExportWardsQueryHandler : IRequestHandler<ExportWardsQuery, string>
    {
        private readonly IWardRepository _repository;

        public ExportWardsQueryHandler(IWardRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportWardsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportWards(cancellationToken);
            return result;
        }
    }
}
