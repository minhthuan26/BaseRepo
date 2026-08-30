using Gateway.Application.Interfaces;
using Gateway.Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.User.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserOutputDTO>
    {
        private readonly IUserRepository _repository;
        public GetUserByIdQueryHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserOutputDTO> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetUserById(request.Data, request.SiteId, cancellationToken);
            return result;
        }
    }
}
