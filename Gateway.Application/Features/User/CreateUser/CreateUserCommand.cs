using Gateway.Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.User.CreateUser
{
    public class CreateUserCommand : IRequest<UserOutputDTO>
    {
        public UserInputDTO Data { get; set; }
    }
}
