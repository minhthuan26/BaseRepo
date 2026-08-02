using AdministrativeUnit.Application.Interfaces;
using MediatR;

namespace AdministrativeUnit.Application.Features.Province.DeleteProvince
{
    public class DeleteProvinceCommandHandler : IRequestHandler<DeleteProvinceCommand, string>
    {
        private readonly IProvinceRepository _repository;

        public DeleteProvinceCommandHandler(IProvinceRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteProvinceCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteProvince(request.Data, cancellationToken);
            return result;
        }
    }
}
