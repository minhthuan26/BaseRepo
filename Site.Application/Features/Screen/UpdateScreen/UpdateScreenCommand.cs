using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Screen.UpdateScreen
{
    public class UpdateScreenCommand : IRequest<ScreenOutputDTO>
    {
        public ScreenInputDTO Data { get; set; }
    }
}
