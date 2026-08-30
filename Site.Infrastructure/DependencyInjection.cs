using CommonLibs.Options;
using CommonLibs.Services;
using CommonLibs.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Site.Application.Interfaces;
using Site.Infrastructure.DBContext;
using Site.Infrastructure.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

namespace Site.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSiteInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Đăng ký các dịch vụ đảm bảo khởi động (EnsureServiceRunning) nếu cần thiết
            services.AddEnsureServiceRunning();

            // 2. Đăng ký các service chung từ CommonLibs
            services.AddCommonServices<SiteDbContext>(configuration, "Site.Application");

            // 3. Đăng ký các Repository nội bộ
            services.AddScoped<JwtSecurityTokenHandler>();
            services.AddScoped<ISiteRepository, SiteRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IFunctionRepository, FunctionRepository>();
            services.AddScoped<IRoleFunctionRepository, RoleFunctionRepository>();
            services.AddScoped<IScreenRepository, ScreenRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            // IUserSiteRepository/UserSiteRepository ĐÃ XOÁ - User giờ chỉ thuộc đúng 1 Site (1-1, gắn thẳng
            // tbl_Users.SiteId), tính năng quản lý quan hệ User-Site kiểu N-N qua tbl_User_Site không còn ý
            // nghĩa sau migration multi-tenant (xem CommonLibs/BaseSQL/multi-tenant/README.md).
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<ISiteModuleRepository, SiteModuleRepository>();

            return services;
        }
    }
}
