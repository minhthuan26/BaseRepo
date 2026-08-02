using CommonLibs.Logging;
using CommonLibs.Middlewares;
using CommonLibs.Options;
using CommonLibs.Services;
using CommonLibs.Utils;
using Gateway.Infrastructure;
using Gateway.Utils;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================================
// ĐĂNG KÝ SERVICES (DI CONTAINER)
// ==========================================================================
builder.AddDynamicLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Gộp cấu hình Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "App API", Version = "v1" });
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

builder.Services.AddGatewayInfrastructure(builder.Configuration);
// Cấu hình Kestrel lắng nghe port 80 và 443
builder.WebHost.ConfigureKestrel(options =>
{
    // Lắng nghe HTTP thông thường
    options.ListenAnyIP(80);

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
// Cấu hình YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // 1. Cấu hình đọc Swagger JSON của chính bản thân Gateway (các controller viết tại Gateway)
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API");

        // 2. Cấu hình đọc Swagger JSON của Service Site thông qua YARP
        // Lưu ý: "/site-swagger" khớp với route "site-swagger-route" trong appsettings.json (ReverseProxy),
        // route này sẽ proxy và rewrite thành /swagger/v1/swagger.json khi gọi xuống Site.
        options.SwaggerEndpoint("/site-swagger/v1/swagger.json", "Site API");

    });
}

app.UseHttpsRedirection();

// 1. BẮT BUỘC: Kích hoạt khả năng định tuyến đường dẫn
app.UseRouting();

// 2. BẮT BUỘC: CORS phải đứng SAU UseRouting và TRƯỚC Authentication/Authorization
app.UseCors("AllowSpecificOrigins");

// 3. BẮT BUỘC: Phải xác thực danh tính (Authentication) TRƯỚC KHI phân quyền (Authorization)
app.UseAuthentication();
app.UseMiddleware<AuthorizationMiddleware>();
app.UseAuthorization();

// 4. BẮT BUỘC: Đóng gói các Endpoint xử lý cuối cùng
app.UseEndpoints(endpoints =>
{
    // Ưu tiên chạy các API Controller viết ngay tại Gateway trước (ví dụ: TestAPIController)
    endpoints.MapControllers();

    // Nếu không khớp Controller nào, YARP sẽ đứng sau "hứng" và bắn xuống Microservices
    endpoints.MapReverseProxy();
});

app.Run();