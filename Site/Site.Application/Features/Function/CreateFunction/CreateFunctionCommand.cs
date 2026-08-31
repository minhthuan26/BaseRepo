using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Function.CreateFunction
{
    public class CreateFunctionCommand : IRequest<FunctionOutputDTO>
    {
        public FunctionInputDTO Data { get; set; }
    }
}
