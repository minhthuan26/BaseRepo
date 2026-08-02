using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Function.CreateFunction
{
    public class CreateFunctionCommand : IRequest<FunctionOutputDTO>
    {
        public FunctionInputDTO Data { get; set; }
    }
}
