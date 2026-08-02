using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Screen.GetScreenById
{
    public class GetScreenByIdQuery : IRequest<ScreenOutputDTO>
    {
        public string Data { get; set; }
    }
}
