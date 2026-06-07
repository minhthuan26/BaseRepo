using Gateway.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.Authorization.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, string>
    {
        private readonly IAuthorizationRepository _repository;

        public LogoutCommandHandler(IAuthorizationRepository repository)
        {
            _repository = repository;
        }
        public async Task<string> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.Logout(cancellationToken);
            return result;
        }
    }
}
