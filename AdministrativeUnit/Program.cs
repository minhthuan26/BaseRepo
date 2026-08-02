using AdministrativeUnit.Infrastructure;
using AdministrativeUnit.Utils;
using CommonLibs.Logging;
using CommonLibs.Middlewares;
using CommonLibs.Options;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================================
// ĐĂNG KÝ SERVICES (DI CONTAINER)
// ==========================================================================
builder.AddDynamicLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger phục vụ việc xuất bản JSON cho Gateway kéo về
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AdministrativeUnit API", Version = "v1" });
    options.CustomSchemaIds(type => type.ToString());

    // Giữ lại cấu hình Bearer để Tester có thể debug trực tiếp tại service nếu cần
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter Access Token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[]{}
        }
    });
    options.OperationFilter<AddRequiredHeaderParameter>();
});

builder.Services.AddAdministrativeUnitInfrastructure(builder.Configuration);

/// Cấu hình Kestrel lắng nghe port 8002
builder.WebHost.ConfigureKestrel(options =>
{
    // Lắng nghe HTTP thông thường
    options.ListenAnyIP(8002);

    // Lắng nghe HTTPS bảo mật (Yêu cầu Server cần cài sẵn chứng chỉ SSL hợp lệ)
    //options.ListenAnyIP(443, listenOptions =>
    //{
    //    listenOptions.UseHttps();
    //});
});
// Cấu hình Bảo mật
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    // Không so khớp literal "User"/"Admin" nữa - chỉ cần user đã đăng nhập và có ít nhất 1 Role claim
    // (Role thật, VD "Ký thuật viên"), việc chặn cụ thể theo Function do RequireFunction/AdminPolicy đảm nhiệm.
    options.AddPolicy("UserOrAdminPolicy", policy => policy.RequireAssertion(context =>
        context.User.Identity?.IsAuthenticated == true
        && context.User.Claims.Any(c => c.Type == ClaimTypes.Role)));
});

// Cấu hình CORS nới lỏng cho môi trường Dev/Gateway gọi nội bộ mượt mà
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

// ==========================================================================
// CẤU HÌNH HTTP REQUEST PIPELINE
// ==========================================================================

app.UseDynamicLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

// Luôn bật Swagger ở Dev để Gateway có thể bóc tách và map route
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
}

// BẬT định tuyến đường dẫn
app.UseRouting();

// CORS đứng sau Routing, trước Auth
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseMiddleware<AuthorizationMiddleware>();
app.UseAuthorization();

// Chỉ Map các API Controller nội tại của chính Service này
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
