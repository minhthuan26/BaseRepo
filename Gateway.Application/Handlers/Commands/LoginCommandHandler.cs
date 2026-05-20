using Gateway.Application.Interfaces;
using Gateway.Application.Commands;
using Gateway.Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Handlers.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthorizationOutputDTO>
    {
        private readonly IGatewayRepository _repository;

        public LoginCommandHandler(IGatewayRepository repository) 
        {
            _repository = repository;
        }
        public async Task<AuthorizationOutputDTO> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.Login(request.Data, cancellationToken);
            return result;
        }
    }
}
