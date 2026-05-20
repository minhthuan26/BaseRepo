using Gateway.Application.Commands;
using Gateway.Application.Interfaces;
using Gateway.Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Handlers.Commands
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, string>
    {
        private readonly IGatewayRepository _repository;

        public LogoutCommandHandler(IGatewayRepository repository)
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
