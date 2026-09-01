using Gateway.Application.Interfaces;
using MediatR;

namespace Gateway.Application.Features.User.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, string>
    {
        private readonly IUserRepository _repository;

        public DeleteUserCommandHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.DeleteUser(request.Data, cancellationToken);
            return result;
        }
    }
}
