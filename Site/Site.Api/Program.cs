using CommonLibs.Logging;
using CommonLibs.Middlewares;
using CommonLibs.Options;
using Microsoft.OpenApi;
using Site.Infrastructure;
using Site.Api.Utils;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Serilog tách riêng khỏi appsettings.json — nạp trước khi AddDynamicLogging() đọc section "Serilog"
builder.Configuration.AddJsonFile("appsettings.Serilog.json", optional: false, reloadOnChange: true);

// ==========================================================================
// ĐĂNG KÝ SERVICES (DI CONTAINER)
// ==========================================================================
builder.AddDynamicLogging();
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
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
    options.OperationFilter<AddRequiredHeaderParameter>();
});

builder.Services.AddSiteInfrastructure(builder.Configuration);

/// Cấu hình Kestrel lắng nghe port 80 và 443
builder.WebHost.ConfigureKestrel(options =>
{
    // Lắng nghe HTTP thông thường
    options.ListenAnyIP(8001);

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
    // ĐÃ BỎ "AdminPolicy" (trước đây policy.RequireRole("Admin")) - so khớp literal RoleName "Admin" không còn
    // đúng nghĩa "Admin toàn hệ thống" từ khi mỗi Site tự có Role "Admin" riêng (multi-tenant): Admin của Site A
    // sẽ vô tình pass policy này trên cả tài nguyên của Site B. KHÔNG thêm lại policy này hay [Authorize(Roles =
    // "Admin")] ở bất kỳ đâu - chặn theo quyền cụ thể dùng RequireFunction (theo đúng Function được Role đó cấp
    // qua tbl_Role_Function), chặn theo đúng 1 Site cụ thể dùng BaseController.IsSiteAdmin(siteId).
    // Không so khớp literal "User"/"Admin" nữa - chỉ cần user đã đăng nhập và có ít nhất 1 Role claim
    // (Role thật, VD "Ký thuật viên"), việc chặn cụ thể theo Function do RequireFunction đảm nhiệm.
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