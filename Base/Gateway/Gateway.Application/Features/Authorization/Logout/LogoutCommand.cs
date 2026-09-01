using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.Authorization.Logout
{
    public class LogoutCommand : IRequest<string>
    {
    }
}
