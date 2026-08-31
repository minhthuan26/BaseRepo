using AutoMapper;
using Site.Contract.DTOs;
using Site.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Mappings
{
    public class SiteMapping : Profile
    {
        public SiteMapping()
        {
            // Function
            CreateMap<FunctionInputDTO, FunctionOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<Function, FunctionOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            // Module
            CreateMap<ModuleInputDTO, ModuleOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<Module, ModuleOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            // Role
            CreateMap<RoleInputDTO, RoleOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<Role, RoleOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            // RoleFunction
            CreateMap<RoleFunctionInputDTO, RoleFunctionOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<RoleFunction, RoleFunctionOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            // Screen
            CreateMap<ScreenInputDTO, ScreenOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<Screen, ScreenOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            // Site
            CreateMap<SiteInputDTO, SiteOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<Domain.Entities.Site, SiteOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            // UserRole
            CreateMap<UserRoleInputDTO, UserRoleOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<UserRole, UserRoleOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            // SiteModule
            CreateMap<ProvisionSiteModuleInputDTO, SiteModuleOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<Domain.Entities.SiteModule, SiteModuleOutputDTO>()
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
