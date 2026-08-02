using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Screen.CreateScreen
{
    public class CreateScreenCommand : IRequest<ScreenOutputDTO>
    {
        public ScreenInputDTO Data { get; set; }
    }
}
