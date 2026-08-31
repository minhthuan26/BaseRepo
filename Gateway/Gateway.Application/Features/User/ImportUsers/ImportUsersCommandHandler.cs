using Gateway.Application.Interfaces;
using Gateway.Contract.DTOs;
using MediatR;

namespace Gateway.Application.Features.User.ImportUsers
{
    public class ImportUsersCommandHandler : IRequestHandler<ImportUsersCommand, List<UserOutputDTO>>
    {
        private readonly IUserRepository _repository;

        public ImportUsersCommandHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserOutputDTO>> Handle(ImportUsersCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.ImportUsers(request.Data, request.FileName, request.SiteId, cancellationToken);
            return result;
        }
    }
}
