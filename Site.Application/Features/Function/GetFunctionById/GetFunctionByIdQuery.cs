using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.Function.GetFunctionById
{
    public class GetFunctionByIdQuery : IRequest<FunctionOutputDTO>
    {
        public string Data { get; set; }
    }
}
