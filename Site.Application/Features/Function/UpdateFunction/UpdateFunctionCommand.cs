using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Function.UpdateFunction
{
    public class UpdateFunctionCommand : IRequest<FunctionOutputDTO>
    {
        public FunctionInputDTO Data { get; set; }
    }
}
