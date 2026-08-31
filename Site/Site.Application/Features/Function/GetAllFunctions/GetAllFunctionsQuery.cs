using CommonLibs.DTOs;
using MediatR;
using Site.Contract.DTOs;

namespace Site.Application.Features.Function.GetAllFunctions
{
    public class GetAllFunctionsQuery : IRequest<BaseFilterOutput<FunctionOutputDTO>>
    {
        public FunctionFilterDTO Filter { get; set; }
    }
}
