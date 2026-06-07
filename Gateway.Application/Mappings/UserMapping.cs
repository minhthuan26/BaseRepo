using AutoMapper;
using Gateway.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Mappings
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<UserInputDTO, UserOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
