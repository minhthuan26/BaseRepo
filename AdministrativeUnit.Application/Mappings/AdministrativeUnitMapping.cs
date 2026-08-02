using AdministrativeUnit.Domain.DTOs;
using AdministrativeUnit.Domain.Entities;
using AutoMapper;

namespace AdministrativeUnit.Application.Mappings
{
    public class AdministrativeUnitMapping : Profile
    {
        public AdministrativeUnitMapping()
        {
            // Province
            CreateMap<ProvinceInputDTO, ProvinceOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<Province, ProvinceOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            // Ward
            CreateMap<WardInputDTO, WardOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<Ward, WardOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
