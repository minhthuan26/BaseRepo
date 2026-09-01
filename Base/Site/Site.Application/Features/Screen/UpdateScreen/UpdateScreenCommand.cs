using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Screen.UpdateScreen
{
    public class UpdateScreenCommand : IRequest<ScreenOutputDTO>
    {
        public ScreenInputDTO Data { get; set; }
    }
}
