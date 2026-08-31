using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.UserRole.GetRolesByUser
{
    public class GetRolesByUserQueryHandler : IRequestHandler<GetRolesByUserQuery, List<RoleOutputDTO>>
    {
        private readonly IUserRoleRepository _repository;

        public GetRolesByUserQueryHandler(IUserRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RoleOutputDTO>> Handle(GetRolesByUserQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetRolesByUser(request.UserId, request.SiteId, cancellationToken);
            return result;
        }
    }
}
