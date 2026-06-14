using CommonLibs.Logging;
using CommonLibs.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================================
// ĐĂNG KÝ SERVICES (DI CONTAINER)
// ==========================================================================

// Ghi log tập trung hệ thống
builder.AddSharedSerilog();

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("Application"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger phục vụ việc xuất bản JSON cho Gateway kéo về
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Site API", Version = "v1" });
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
});

builder.Services.AddHttpContextAccessor();

// Cấu hình Xác thực JWT bảo mật cho riêng Service con
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddAuthorization();

// Cấu hình CORS nới lỏng cho môi trường Dev/Gateway gọi nội bộ mượt mà
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Cho phép Gateway hoặc FE gọi trực tiếp khi debug
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

// Luôn bật Swagger ở Dev để Gateway có thể bóc tách và map route
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// BẬT định tuyến đường dẫn
app.UseRouting();

// CORS đứng sau Routing, trước Auth
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

// Chỉ Map các API Controller nội tại của chính Service này
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();