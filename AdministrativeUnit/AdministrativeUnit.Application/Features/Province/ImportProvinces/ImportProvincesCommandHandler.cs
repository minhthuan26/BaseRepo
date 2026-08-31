using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.ImportProvinces
{
    public class ImportProvincesCommandHandler : IRequestHandler<ImportProvincesCommand, List<ProvinceOutputDTO>>
    {
        private readonly IProvinceRepository _repository;

        public ImportProvincesCommandHandler(IProvinceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProvinceOutputDTO>> Handle(ImportProvincesCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportProvinces(request.Data, request.FileName, cancellationToken);
            return result;
        }
    }
}
