using Gateway.Domain.DTOs;
using MediatR;

namespace Gateway.Application.Features.Authorization.RenewToken
{
    public class RenewTokenCommand : IRequest<AuthorizationOutputDTO>
    {
    }
}
