using CommonLibs.Options;
using CommonLibs.Services;
using CommonLibs.Utils;
using Gateway.Application.Interfaces;
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

namespace Gateway.Application.Middlewares
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

        public async Task Invoke(HttpContext context, IJwtTokenService jwtTokenService)
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

                var roleClaim = claims.FirstOrDefault(claim => claim.Type == "role" || claim.Type == ClaimTypes.Role);
                var userIdClaim = claims.FirstOrDefault(claim => claim.Type == "id" || claim.Type == ClaimTypes.NameIdentifier);

                if (roleClaim == null || userIdClaim == null)
                {
                    throw new AppException("Token payload is not valid.", HttpStatusCode.BadRequest);
                }

                string roleName = roleClaim.Value;
                string userId = userIdClaim.Value;

                // ==================== THÊM ĐOẠN NÀY ĐỂ FIX LỖI 401 ====================
                // Tạo danh tính chính thức để các Middleware hệ thống (.NET Core Authorization) công nhận
                var identity = new ClaimsIdentity(claims, "Bearer");
                context.User = new ClaimsPrincipal(identity);
                // ======================================================================

                // 5. PHÂN QUYỀN THEO ROUTE YARP
                var authData = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>();
                if (authData != null && !string.IsNullOrEmpty(authData.Roles))
                {
                    var acceptedRoles = authData.Roles.Split(',');
                    if (!acceptedRoles.Contains(roleName))
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
                context.Request.Headers["X-User-Role"] = roleName;
                context.Request.Headers["X-Validated-Device-Id"] = deviceId;
                context.Request.Headers["X-Validated-Device-Name"] = deviceName;

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
