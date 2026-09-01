using AdministrativeUnit.Application.Interfaces;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.ExportProvinces
{
    public class ExportProvincesQueryHandler : IRequestHandler<ExportProvincesQuery, string>
    {
        private readonly IProvinceRepository _repository;

        public ExportProvincesQueryHandler(IProvinceRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportProvincesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportProvinces(cancellationToken);
            return result;
        }
    }
}
