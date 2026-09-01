using CommonLibs.Options;
using CommonLibs.Services;
using CommonLibs.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Booking.Infrastructure.DBContext;
using Booking.Application.Interfaces;
using Booking.Infrastructure.Repositories;
using System.IdentityModel.Tokens.Jwt;

namespace Booking.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEnsureServiceRunning();
            services.AddCommonServices<BookingDbContext>(configuration, "Booking.Application");
            services.AddScoped<JwtSecurityTokenHandler>();

            services.AddScoped<IFieldTypeRepository, FieldTypeRepository>();
            services.AddScoped<IFieldRepository, FieldRepository>();
            // Repository registrations added by later tasks (PricingRule).

            return services;
        }
    }
}
