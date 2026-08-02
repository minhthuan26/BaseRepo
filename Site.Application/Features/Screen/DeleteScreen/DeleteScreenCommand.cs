using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Screen.DeleteScreen
{
    public class DeleteScreenCommand : IRequest<string>
    {
        public DeleteScreenInputDTO Data { get; set; }
    }
}
