using CommonLibs.Logging;
using CommonLibs.Services;
using CommonLibs.Utils;
using Gateway.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================================
// ĐĂNG KÝ SERVICES (DI CONTAINER)
// ==========================================================================
builder.AddSharedSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Gộp cấu hình Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway API", Version = "v1" });
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
});

builder.Services.AddGatewayInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<GrpcClientHelper>();
builder.Services.AddHttpClient<HttpServiceHttpClient>();

// Cấu hình YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Cấu hình Bảo mật
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddAuthorization();

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

app.UseSharedSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 1. BẮT BUỘC: Kích hoạt khả năng định tuyến đường dẫn
app.UseRouting();

// 2. BẮT BUỘC: CORS phải đứng SAU UseRouting và TRƯỚC Authentication/Authorization
app.UseCors("AllowSpecificOrigins");

// 3. BẮT BUỘC: Phải xác thực danh tính (Authentication) TRƯỚC KHI phân quyền (Authorization)
app.UseAuthentication();
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