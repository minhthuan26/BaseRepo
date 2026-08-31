using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Function.GetFunctionById
{
    public class GetFunctionByIdQuery : IRequest<FunctionOutputDTO>
    {
        public string Data { get; set; }
    }
}
