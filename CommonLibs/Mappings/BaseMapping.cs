using AutoMapper;
using CommonLibs.DTOs;
using CommonLibs.Entities;

namespace CommonLibs.Mappings
{
    public class BaseMapping : Profile
    {
        public BaseMapping()
        {
            CreateMap<BaseEntity, BaseDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
