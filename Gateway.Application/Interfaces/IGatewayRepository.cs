using Gateway.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Interfaces
{
    public interface IGatewayRepository
    {
        Task<AuthorizationOutputDTO> Login(AuthorizationInputDTO input, CancellationToken cancellationToken);
        Task<string> Logout(CancellationToken cancellationToken);
        Task<AuthorizationOutputDTO> RenewToken(CancellationToken cancellationToken);
        Task<List<Claim>> VerifyToken(string token, string key);
    }
}
