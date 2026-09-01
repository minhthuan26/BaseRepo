using Gateway.Application.Interfaces;
using Gateway.Contract.DTOs;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Application.Features.User.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, UserOutputDTO>
    {
        private readonly IUserRepository _repository;

        public ChangePasswordCommandHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserOutputDTO> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            return await _repository.ChangePassword(request.Data, cancellationToken);
        }
    }
}
