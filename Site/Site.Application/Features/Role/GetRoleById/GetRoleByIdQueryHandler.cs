using MediatR;
using Site.Application.Interfaces;
using Site.Contract.DTOs;

namespace Site.Application.Features.Role.GetRoleById
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleOutputDTO>
    {
        private readonly IRoleRepository _repository;

        public GetRoleByIdQueryHandler(IRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<RoleOutputDTO> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetRoleById(request.Data, request.SiteId, cancellationToken);
            return result;
        }
    }
}
