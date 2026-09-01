using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Screen.GetScreenById
{
    public class GetScreenByIdQuery : IRequest<ScreenOutputDTO>
    {
        public string Data { get; set; }
    }
}
