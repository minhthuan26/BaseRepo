using AutoMapper;
using Booking.Contract.DTOs;
using Booking.Domain.Entities;

namespace Booking.Application.Mappings
{
    public class BookingMapping : Profile
    {
        public BookingMapping()
        {
            CreateMap<FieldType, FieldTypeOutputDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<FieldTypeInputDTO, FieldTypeOutputDTO>();
        }
    }
}
