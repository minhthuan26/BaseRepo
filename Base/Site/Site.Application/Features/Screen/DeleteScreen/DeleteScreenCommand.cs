using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Screen.DeleteScreen
{
    public class DeleteScreenCommand : IRequest<string>
    {
        public DeleteScreenInputDTO Data { get; set; }
    }
}
