using CommonLibs.Options;
using CommonLibs.Services;
using CommonLibs.Utils;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommonLibs.Middlewares
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly AppOptions _options;

        public AuthorizationMiddleware(RequestDelegate next, IConfiguration configuration, IOptions<AppOptions> options)
        {
            _next = next;
            _configuration = configuration;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context, IJwtTokenService jwtTokenService, ISqlConnectionFactory sqlConnectionFactory, IRedisService redisService)
        {
            try
            {
                // 1. Bỏ qua nếu là Route cho phép ẩn danh công khai (Anonymous công khai tại Gateway)
                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    // Check 1: Nếu là endpoint nội bộ của Gateway có đánh dấu [AllowAnonymous]
                    var hasAllowAnonymous = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null;

                    // Check 2: Lấy thông tin Policy cấu hình trên YARP Route
                    var authorizeData = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>();

                    // NẾU: Có thuộc tính ẩn danh HOẶC YARP route này không yêu cầu bất kỳ Policy bảo mật nào
                    if (hasAllowAnonymous || authorizeData == null)
                    {
                        await _next(context); // Cho đi tiếp (Bỏ qua check token/device)
                        return;
                    }
                }
                else
                {
                    // Nếu endpoint thực sự null (không tìm thấy tuyến đường nào cả)
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { code = HttpStatusCode.NotFound, message = "Route không tồn tại trên Gateway." });
                    return;
                }

                // 2. Xác thực cấu trúc Header Authorization
                string bearerToken = context.Request.Headers["Authorization"];
                if (string.IsNullOrEmpty(bearerToken) || !bearerToken.StartsWith("Bearer "))
                {
                    throw new AppException("Unauthorized.", HttpStatusCode.Unauthorized);
                }

                string accessToken = bearerToken.Split(" ")[1];

                // 3. KIỂM TRA THIẾT BỊ (DEVICE ID & DEVICE NAME)
                if (!context.Request.Headers.TryGetValue("X-Device-ID", out var deviceId) || string.IsNullOrEmpty(deviceId))
                {
                    throw new AppException("Missing X-Device-ID header.", HttpStatusCode.BadRequest);
                }

                if (!context.Request.Headers.TryGetValue("X-Device-Name", out var deviceName) || string.IsNullOrEmpty(deviceName))
                {
                    throw new AppException("Missing X-Device-Name header.", HttpStatusCode.BadRequest);
                }

                // 4. Xác thực Token thông qua Auth Service
                List<Claim> claims = await jwtTokenService.VerifyToken(accessToken, _options.ActualAccessKey);

                var roleNames = claims.Where(claim => claim.Type == "role" || claim.Type == ClaimTypes.Role).Select(claim => claim.Value).ToList();
                var userIdClaim = claims.FirstOrDefault(claim => claim.Type == "id" || claim.Type == ClaimTypes.NameIdentifier);
                // "auto_id" và "site_id" được Login() (Gateway.Infrastructure/Repositories/AuthorizationRepository.cs)
                // gán vào token - auto_id là khoá cache Redis (UserId giờ chỉ unique trong phạm vi 1 Site nên không còn
                // dùng UserId làm khoá cache được, dễ đụng nhau giữa 2 Site khác nhau), site_id dùng để lọc dữ liệu khi
                // Redis cache miss phải query thẳng SQL.
                var autoIdClaim = claims.FirstOrDefault(claim => claim.Type == "auto_id");
                var siteIdClaim = claims.FirstOrDefault(claim => claim.Type == "site_id");

                if (roleNames.Count == 0 || userIdClaim == null || autoIdClaim == null || siteIdClaim == null)
                {
                    throw new AppException("Token payload is not valid.", HttpStatusCode.BadRequest);
                }

                string userId = userIdClaim.Value;
                string autoId = autoIdClaim.Value;
                string siteId = siteIdClaim.Value;

                // ==================== THÊM ĐOẠN NÀY ĐỂ FIX LỖI 401 ====================
                // Tạo danh tính chính thức để các Middleware hệ thống (.NET Core Authorization) công nhận
                var identity = new ClaimsIdentity(claims, "Bearer");
                context.User = new ClaimsPrincipal(identity);
                // ======================================================================

                // 5. PHÂN QUYỀN THEO ROUTE YARP
                // CẢNH BÁO: KHÔNG cấu hình "Roles": "Admin" (hay bất kỳ tên Role nào khác) trên route YARP trong
                // appsettings.json (hiện chưa route nào dùng field này - chỉ dùng "AuthorizationPolicy"). so khớp
                // RoleName ở đây cũng literal/site-blind y hệt lỗi "Admin toàn hệ thống" đã fix (xem README
                // CommonLibs/BaseSQL/multi-tenant/) - Admin của Site A sẽ tự động pass check này trên route lẽ ra
                // chỉ dành cho Site B. Muốn giới hạn theo quyền cụ thể, dùng [RequireFunction(...)] trên action.
                var authData = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>();
                if (authData != null && !string.IsNullOrEmpty(authData.Roles))
                {
                    var acceptedRoles = authData.Roles.Split(',');
                    if (!roleNames.Any(roleName => acceptedRoles.Contains(roleName)))
                    {
                        throw new AppException("Access denied. You do not have permission.", HttpStatusCode.Forbidden);
                    }
                }

                // 6. Kiểm tra chéo Token và Device
                var tokenDeviceIdClaim = claims.FirstOrDefault(claim => claim.Type == "device_id")?.Value;
                if (!string.IsNullOrEmpty(tokenDeviceIdClaim) && tokenDeviceIdClaim != deviceId)
                {
                    throw new AppException("Token is not valid for this device.", HttpStatusCode.Forbidden);
                }

                // 7. ENRICH DATA (Đóng gói thông tin sạch đẩy xuống Service con)
                context.Request.Headers["X-User-Id"] = userId;
                // Site mà user hiện tại thuộc về (ranh giới tenant) - service con dùng để tự lọc dữ liệu theo Site
                // của chính user (VD SiteModuleController.GetMySiteModules) mà không cần tin theo SiteId client tự
                // truyền lên trong query/body.
                context.Request.Headers["X-Site-Id"] = siteId;
                // Uri.EscapeDataString: RoleName lấy từ DB (VD "Ký thuật viên") có thể chứa ký tự Unicode -
                // HTTP header value bắt buộc chỉ ASCII, gửi thẳng sẽ làm YARP forward request tới Site lỗi
                // "Request headers must contain only ASCII characters" (502).
                context.Request.Headers["X-User-Role"] = Uri.EscapeDataString(string.Join(",", roleNames));
                context.Request.Headers["X-Validated-Device-Id"] = deviceId;
                context.Request.Headers["X-Validated-Device-Name"] = deviceName;

                // 8. NẠP DANH SÁCH FUNCTION ĐƯỢC PHÉP — ưu tiên đọc từ Redis (đã được Login() cache sẵn), chỉ query
                // SQL trực tiếp (User_Role -> Roles -> Role_Function -> Functions) khi cache miss/hết hạn, sau đó
                // ghi lại vào Redis cho các request kế tiếp. Chưa dùng để chặn cứng từng endpoint (Function hiện
                // chưa gắn với route cụ thể nào) — chỉ nạp sẵn hạ tầng để controller/service tự kiểm tra khi cần.
                string cacheKey = UserPermissionCache.GetCacheKey(autoId);
                var cached = await redisService.GetAsync<UserPermissionCache>(cacheKey);
                List<string> allowedFunctionIds;
                if (cached != null)
                {
                    allowedFunctionIds = cached.Functions;
                }
                else
                {
                    using var connection = sqlConnectionFactory.CreateConnection();
                    // Bắt buộc lọc thêm And UR.SiteId = @SiteId: UserId không còn unique toàn hệ thống (chỉ unique
                    // trong phạm vi 1 Site), thiếu điều kiện này khi cache miss có thể vô tình gộp cả Function của
                    // một User trùng UserId ở Site khác vào - rò rỉ quyền chéo Site.
                    allowedFunctionIds = (await connection.QueryAsync<string>(@"
                            Select Distinct RF.FunctionId
                            From tbl_User_Role UR
                            Inner Join tbl_Roles R On R.RoleId = UR.RoleId
                            Inner Join tbl_Role_Function RF On RF.RoleId = R.RoleId
                            Where UR.UserId = @UserId And UR.SiteId = @SiteId And UR.Status = 1 And R.Status = 1 And RF.Status = 1
                        ", new { UserId = userId, SiteId = siteId })).ToList();

                    await redisService.SetAsync(
                        cacheKey,
                        new UserPermissionCache { Roles = roleNames, Functions = allowedFunctionIds },
                        TimeSpan.FromHours(1));
                }

                context.Items["allowedFunctions"] = allowedFunctionIds;
                context.Request.Headers["X-User-Functions"] = Uri.EscapeDataString(string.Join(",", allowedFunctionIds));

                // 9. CHẶN THEO FUNCTION CỤ THỂ — action nào gắn [RequireFunction("FunctionId")] thì user phải có
                // đúng FunctionId đó trong allowedFunctionIds (gán qua Role -> Role_Function) mới được đi tiếp.
                // KHÔNG còn khái niệm "Admin toàn hệ thống" nào được bỏ qua kiểm tra này (đã bỏ chủ ý - trước đây
                // so khớp literal RoleName == "Admin" để bypass, nhưng từ khi mỗi Site tự có Role "Admin" riêng
                // (multi-tenant), việc này vô tình cho phép Admin của Site A bỏ qua RequireFunction trên tài
                // nguyên của Site B. Role "Admin" của mỗi Site giờ phải được cấp Function tường minh qua
                // tbl_Role_Function như mọi Role khác - xem CommonLibs/BaseSQL/multi-tenant/07_backfill_admin_role_functions.sql
                // để không bị mất quyền đột ngột ngay khi bật đợt fix này.
                var requireFunction = endpoint?.Metadata.GetMetadata<RequireFunctionAttribute>();
                if (requireFunction != null && !allowedFunctionIds.Contains(requireFunction.FunctionId))
                {
                    throw new AppException($"Access denied. Missing required function permission: '{requireFunction.FunctionId}'.", HttpStatusCode.Forbidden);
                }

                context.Items["claims"] = claims;

                await _next(context);
            }
            catch (AppException ex)
            {
                var response = new Response<object>
                {
                    Success = false,
                    Code = (HttpStatusCode)(int)ex.Code,
                    Message = ex.Message,
                    Data = null
                };

                context.Response.StatusCode = (int)ex.Code;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                   JsonSerializer.Serialize(response, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping })
                );
            }
        }
    }
}
