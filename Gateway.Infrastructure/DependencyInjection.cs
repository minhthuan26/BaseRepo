using CommonLibs.Options;
using CommonLibs.Services;
using CommonLibs.Utils;
using EncryptionLibrary;
using Gateway.Application.Interfaces;
using Gateway.Infrastructure.DBContext;
using Gateway.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;

namespace Gateway.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Đăng ký các service chung từ CommonLibs
            services.AddCommonServices<GatewayDbContext>(configuration, "Gateway.Application");

            // 2. Đăng ký các Repository nội bộ
            services.AddScoped<JwtSecurityTokenHandler>();
            services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // 4. Đăng ký các dịch vụ đảm bảo khởi động (EnsureServiceRunning) nếu cần thiết
            services.AddEnsureServiceRunning("GatewayServiceDependencies");

            return services;
        }
    }
}
