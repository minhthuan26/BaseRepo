using CommonLibs.DTOs;
using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Screen.GetAllScreens
{
    public class GetAllScreensQuery : IRequest<BaseFilterOutput<ScreenOutputDTO>>
    {
        public ScreenFilterDTO Filter { get; set; }
    }
}
