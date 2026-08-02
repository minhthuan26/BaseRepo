using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Screen.ImportScreens
{
    public class ImportScreensCommand : IRequest<List<ScreenOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
