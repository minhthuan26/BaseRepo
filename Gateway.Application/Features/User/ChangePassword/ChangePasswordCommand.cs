using Gateway.Domain.DTOs;
using MediatR;

namespace Gateway.Application.Features.User.ChangePassword
{
    public class ChangePasswordCommand : IRequest<UserOutputDTO>
    {
        public ChangePasswordInputDTO Data { get; set; }
    }
}
