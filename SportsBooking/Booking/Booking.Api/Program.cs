using Booking.Infrastructure;
using Booking.Api.Utils;
using CommonLibs.Logging;
using CommonLibs.Middlewares;
using CommonLibs.Options;
using Microsoft.OpenApi;
using System.Security.Claims;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Serilog.json", optional: false, reloadOnChange: true);

builder.AddDynamicLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Booking API", Version = "v1" });
    options.CustomSchemaIds(type => type.ToString());

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter Access Token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
    options.OperationFilter<AddRequiredHeaderParameter>();
});

builder.Services.AddBookingInfrastructure(builder.Configuration);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8003);
});

builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOrAdminPolicy", policy => policy.RequireAssertion(context =>
        context.User.Identity?.IsAuthenticated == true
        && context.User.Claims.Any(c => c.Type == ClaimTypes.Role)));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseDynamicLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}

app.UseRouting();

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseMiddleware<AuthorizationMiddleware>();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
