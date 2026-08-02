using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Infrastructure.DBContext;
using AdministrativeUnit.Infrastructure.Repositories;
using CommonLibs.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

namespace AdministrativeUnit.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAdministrativeUnitInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Đăng ký các dịch vụ đảm bảo khởi động (EnsureServiceRunning) nếu cần thiết
            services.AddEnsureServiceRunning();

            // 2. Đăng ký các service chung từ CommonLibs
            services.AddCommonServices<AdministrativeUnitDbContext>(configuration, "AdministrativeUnit.Application");

            // 3. Đăng ký các Repository nội bộ
            services.AddScoped<JwtSecurityTokenHandler>();
            services.AddScoped<IProvinceRepository, ProvinceRepository>();
            services.AddScoped<IWardRepository, WardRepository>();

            return services;
        }
    }
}
