using CommonLibs.DTOs;
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.GetAllFieldTypes
{
    public class GetAllFieldTypesQuery : IRequest<BaseFilterOutput<FieldTypeOutputDTO>>
    {
        public FieldTypeFilterDTO Filter { get; set; } = null!;
    }
}
