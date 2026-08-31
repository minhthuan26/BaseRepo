using CommonLibs.DTOs;
using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Role.GetAllRoles
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, BaseFilterOutput<RoleOutputDTO>>
    {
        private readonly IRoleRepository _repository;

        public GetAllRolesQueryHandler(IRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<RoleOutputDTO>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllRoles(request.Filter, cancellationToken);
            return result;
        }
    }
}
