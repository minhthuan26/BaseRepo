using Gateway.Domain.DTOs;
using MediatR;

namespace Gateway.Application.Features.User.UpdateUser
{
    public class UpdateUserCommand : IRequest<UserOutputDTO>
    {
        public UserInputDTO Data { get; set; }
    }
}
