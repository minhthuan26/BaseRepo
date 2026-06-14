using CommonLibs.DTOs;
using Gateway.Application.Interfaces;
using Gateway.Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.User.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, BaseFilterOutput<UserOutputDTO>>
    {
        private readonly IUserRepository _repository;
        public GetAllUsersQueryHandler(IUserRepository repository)
        {
            _repository = repository;
        }
        public async Task<BaseFilterOutput<UserOutputDTO>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllUsers(request.Filter, cancellationToken);
            return result;
        }
    }
}
