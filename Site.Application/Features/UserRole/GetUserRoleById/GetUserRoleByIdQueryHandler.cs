using MediatR;
using Site.Application.Interfaces;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserRole.GetUserRoleById
{
    public class GetUserRoleByIdQueryHandler : IRequestHandler<GetUserRoleByIdQuery, UserRoleOutputDTO>
    {
        private readonly IUserRoleRepository _repository;

        public GetUserRoleByIdQueryHandler(IUserRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserRoleOutputDTO> Handle(GetUserRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetUserRoleById(request.UserId, request.RoleId, cancellationToken);
            return result;
        }
    }
}
