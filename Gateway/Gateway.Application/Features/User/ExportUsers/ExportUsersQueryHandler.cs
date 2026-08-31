using Gateway.Application.Interfaces;
using MediatR;

namespace Gateway.Application.Features.User.ExportUsers
{
    public class ExportUsersQueryHandler : IRequestHandler<ExportUsersQuery, string>
    {
        private readonly IUserRepository _repository;

        public ExportUsersQueryHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ExportUsersQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.ExportUsers(request.SiteId, cancellationToken);
            return result;
        }
    }
}
