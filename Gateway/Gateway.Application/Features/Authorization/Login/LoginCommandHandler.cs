using Gateway.Application.Interfaces;
using Gateway.Contract.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.Authorization.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthorizationOutputDTO>
    {
        private readonly IAuthorizationRepository _repository;

        public LoginCommandHandler(IAuthorizationRepository repository)
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
