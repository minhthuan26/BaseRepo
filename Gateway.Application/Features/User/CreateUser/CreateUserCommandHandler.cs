using Gateway.Application.Interfaces;
using Gateway.Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.User.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserOutputDTO>
    {
        private readonly IUserRepository _repository;

        public CreateUserCommandHandler(IUserRepository repository)
        {
            _repository = repository;
        }
        public async Task<UserOutputDTO> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateUser(request.Data, cancellationToken);
            return result;
        }
    }
}
