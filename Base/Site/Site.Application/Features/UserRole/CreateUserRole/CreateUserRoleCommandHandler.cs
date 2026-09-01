using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.UserRole.CreateUserRole
{
    public class CreateUserRoleCommandHandler : IRequestHandler<CreateUserRoleCommand, UserRoleOutputDTO>
    {
        private readonly IUserRoleRepository _repository;

        public CreateUserRoleCommandHandler(IUserRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserRoleOutputDTO> Handle(CreateUserRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateUserRole(request.Data, cancellationToken);
            return result;
        }
    }
}
