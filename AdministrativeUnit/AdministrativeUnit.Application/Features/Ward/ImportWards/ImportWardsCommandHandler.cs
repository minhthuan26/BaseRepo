using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.ImportWards
{
    public class ImportWardsCommandHandler : IRequestHandler<ImportWardsCommand, List<WardOutputDTO>>
    {
        private readonly IWardRepository _repository;

        public ImportWardsCommandHandler(IWardRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<WardOutputDTO>> Handle(ImportWardsCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportWards(request.Data, request.FileName, cancellationToken);
            return result;
        }
    }
}
