using Gateway.Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.Authorization.Login
{
    public class LoginCommand : IRequest<AuthorizationOutputDTO>
    {
        public AuthorizationInputDTO Data { get; set; }
    }
}
