using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Function.DeleteFunction
{
    public class DeleteFunctionCommand : IRequest<string>
    {
        public DeleteFunctionInputDTO Data { get; set; }
    }
}
