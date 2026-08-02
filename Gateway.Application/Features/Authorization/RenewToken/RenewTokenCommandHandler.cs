using Gateway.Application.Interfaces;
using Gateway.Domain.DTOs;
using MediatR;

namespace Gateway.Application.Features.Authorization.RenewToken
{
    public class RenewTokenCommandHandler : IRequestHandler<RenewTokenCommand, AuthorizationOutputDTO>
    {
        private readonly IAuthorizationRepository _repository;

        public RenewTokenCommandHandler(IAuthorizationRepository repository)
        {
            _repository = repository;
        }
        public async Task<AuthorizationOutputDTO> Handle(RenewTokenCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.RenewToken(cancellationToken);
            return result;
        }
    }
}
