using CommonLibs.DTOs;
using Gateway.Contract.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.User.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<BaseFilterOutput<UserOutputDTO>>
    {
        public UserFilterDTO Filter { get; set; }
    }
}
