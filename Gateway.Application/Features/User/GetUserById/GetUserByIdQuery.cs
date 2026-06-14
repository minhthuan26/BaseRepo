using Gateway.Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.User.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserOutputDTO>
    {
        public string Data { get; set; }
    }
}
