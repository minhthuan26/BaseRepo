using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Screen.CreateScreen
{
    public class CreateScreenCommand : IRequest<ScreenOutputDTO>
    {
        public ScreenInputDTO Data { get; set; }
    }
}
