using CommonLibs.DTOs;
using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.UserRole.GetAllUserRoles
{
    public class GetAllUserRolesQueryHandler : IRequestHandler<GetAllUserRolesQuery, BaseFilterOutput<UserRoleOutputDTO>>
    {
        private readonly IUserRoleRepository _repository;

        public GetAllUserRolesQueryHandler(IUserRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<UserRoleOutputDTO>> Handle(GetAllUserRolesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllUserRoles(request.Filter, cancellationToken);
            return result;
        }
    }
}
