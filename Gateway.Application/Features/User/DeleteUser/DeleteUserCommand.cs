using Gateway.Domain.DTOs;
using MediatR;

namespace Gateway.Application.Features.User.DeleteUser
{
    public class DeleteUserCommand : IRequest<string>
    {
        public DeleteUserInputDTO Data { get; set; }
    }
}
