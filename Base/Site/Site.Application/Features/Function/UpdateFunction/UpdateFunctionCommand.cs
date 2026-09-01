using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Function.UpdateFunction
{
    public class UpdateFunctionCommand : IRequest<FunctionOutputDTO>
    {
        public FunctionInputDTO Data { get; set; }
    }
}
