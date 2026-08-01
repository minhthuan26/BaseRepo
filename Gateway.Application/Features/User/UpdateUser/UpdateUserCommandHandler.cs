using Gateway.Application.Interfaces;
using Gateway.Domain.DTOs;
using MediatR;

namespace Gateway.Application.Features.User.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserOutputDTO>
    {
        private readonly IUserRepository _repository;

        public UpdateUserCommandHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserOutputDTO> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.UpdateUser(request.Data, cancellationToken);
            return result;
        }
    }
}
