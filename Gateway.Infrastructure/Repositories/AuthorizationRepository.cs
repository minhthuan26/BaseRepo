using AutoMapper;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Services;
using CommonLibs.Utils;
using EncryptionLibrary;
using Gateway.Application.Interfaces;
using Gateway.Domain.DTOs;
using Gateway.Domain.Entities;
using Gateway.Infrastructure.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Repositories
{
    public class AuthorizationRepository : BaseRepository<AuthorizationRepository>, IAuthorizationRepository
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly GatewayDbContext _dbContext;
        private readonly IRedisService _redisService;
        private readonly string _errorPrefix = "Authorization_Repo_";

        public AuthorizationRepository(IMapper mapper,
            ILogger<AuthorizationRepository> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContext,
            JwtSecurityTokenHandler jwtSecurityTokenHandler,
            GatewayDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService, IRedisService redisService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
            _dbContext = dbContext;
            _redisService = redisService;
        }

        #region Stored Procedure
        
        #endregion

        #region Repository Methods
        /// <summary>
        /// Thực hiện đăng nhập bằng cách xác thực UserId và Password, sau đó tạo access token và refresh token cho tài khoản. Access token có thời hạn ngắn (10 giờ) để sử dụng cho các yêu cầu API, trong khi refresh token có thời hạn dài hơn (7 ngày) để làm mới access token khi hết hạn. Refresh token được lưu trữ trong database để quản lý và xác thực khi làm mới token.
        /// </summary>
        /// <param name="input">Truyền UserId và Password</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        public async Task<AuthorizationOutputDTO> Login(AuthorizationInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                // Kiểm tra input
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}Login-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrEmpty(input.UserId) || string.IsNullOrEmpty(input.Password))
                {
                    throw new AppException($"[{_errorPrefix}Login-02] UserId or Password is null or empty.", HttpStatusCode.BadRequest);
                }

                // Site = ranh giới tenant (xem tài liệu thiết kế) - phải xác định Site TRƯỚC khi tra tài khoản,
                // vì UserId chỉ còn unique trong phạm vi 1 Site, không còn unique toàn hệ thống.
                var siteId = await ResolveSiteId(input.SiteId, cancellationToken);

                // Xác thực tài khoản - so khớp đúng cả UserId, Password lẫn SiteId đã xác định ở trên.
                var user = await _dbContext.QueryFirstOrDefaultAsync<User>(@"
                        Select * From tbl_Users
                        Where UserId = @UserId And SiteId = @SiteId And Password = @Password
                    ", new
                    {
                        UserId = input.UserId,
                        SiteId = siteId,
                        Password = EncryptionUtils.EncryptData(input.Password, _options.ApplicationName, _options.ApplicationId)
                    },
                    cancellationToken);
                if (user == null)
                {
                    throw new AppException($"[{_errorPrefix}Login-03] Invalid UserId, Password or SiteId.", HttpStatusCode.Unauthorized);
                }

                // Lấy đúng các Role đang active mà user thực sự được gán qua tbl_User_Role (không hard-code, không suy đoán).
                // Lọc thêm theo SiteId (denormalize từ tbl_Roles, xem migration 05_add_siteid_to_role_tables.sql) - ràng
                // buộc nghiệp vụ là Role.SiteId luôn khớp User.SiteId, lọc thêm ở đây là lưới an toàn thứ hai, tránh trường
                // hợp UserId trùng tên giữa 2 Site vô tình join chéo dữ liệu Role của Site khác.
                var roleAssignments = (await _dbContext.QueryAsync<RoleAssignment>(@"
                        Select R.RoleName, R.SiteId
                        From tbl_User_Role UR
                        Inner Join tbl_Roles R On R.RoleId = UR.RoleId
                        Where UR.UserId = @UserId And UR.SiteId = @SiteId And UR.Status = 1 And R.Status = 1
                    ", new { UserId = user.UserId, SiteId = user.SiteId }, cancellationToken)).ToList();

                var roleNames = roleAssignments.Select(ra => ra.RoleName).ToList();

                // SiteId của các Site mà user có Role tên "Admin" (không phân biệt hoa/thường) -> Admin riêng của site đó.
                // Vì roleAssignments giờ đã lọc theo đúng SiteId của user, danh sách này chỉ còn có thể chứa tối đa 1
                // phần tử (chính SiteId của user) - giữ nguyên dạng List/claim "admin_site" để không phá vỡ IsSiteAdmin()
                // đang đọc claim này ở BaseController. Claim "admin_site" CHỈ dùng để BaseController.IsSiteAdmin(siteId)
                // so khớp đúng 1 Site cụ thể (không phải bypass toàn hệ thống) - RequireFunction không đọc claim này.
                var adminSiteIds = roleAssignments
                    .Where(ra => ra.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    .Select(ra => ra.SiteId)
                    .Distinct()
                    .ToList();

                // Lấy các Function mà user được phép thao tác, suy ra từ toàn bộ Role đang active ở trên (cùng lọc SiteId).
                var functionIds = (await _dbContext.QueryAsync<string>(@"
                        Select Distinct RF.FunctionId
                        From tbl_User_Role UR
                        Inner Join tbl_Roles R On R.RoleId = UR.RoleId
                        Inner Join tbl_Role_Function RF On RF.RoleId = R.RoleId
                        Where UR.UserId = @UserId And UR.SiteId = @SiteId And UR.Status = 1 And R.Status = 1 And RF.Status = 1
                    ", new { UserId = user.UserId, SiteId = user.SiteId }, cancellationToken)).ToList();

                // Mỗi Role đang active của user tương ứng 1 claim Role riêng (ClaimsPrincipal hỗ trợ nhiều Role claim cùng lúc).
                var tokenClaims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.UserId),
                    new(ClaimTypes.Name, user.UserName),
                    // site_id: bắt buộc để mọi thao tác về sau (Logout, AuthorizationMiddleware, các Repository khác)
                    // biết user này thuộc Site nào - không còn suy ra được chỉ từ UserId như trước.
                    new("site_id", user.SiteId),
                    // auto_id: khoá unique toàn cục thật sự của User (UserId giờ chỉ unique trong phạm vi 1 Site) -
                    // dùng làm khoá cache Redis thay cho UserId để tránh 2 User trùng UserId ở 2 Site khác nhau vô tình
                    // đọc/ghi chung 1 cache permission của nhau.
                    new("auto_id", user.AutoId.ToString())
                };
                tokenClaims.AddRange(roleNames.Select(roleName => new Claim(ClaimTypes.Role, roleName)));
                tokenClaims.AddRange(adminSiteIds.Select(siteId => new Claim("admin_site", siteId)));

                // Cache Role + Function + AdminSiteIds vào Redis để AuthorizationMiddleware đọc lại thay vì phải query SQL mỗi request.
                // Khoá theo AutoId (xem ghi chú ở claim "auto_id" phía trên) - AuthorizationMiddleware phải đọc cùng khoá này.
                await _redisService.SetAsync(
                    UserPermissionCache.GetCacheKey(user.AutoId.ToString()),
                    new UserPermissionCache { Roles = roleNames, Functions = functionIds, AdminSiteIds = adminSiteIds },
                    TimeSpan.FromHours(1));

                // Tạo access token và refresh token
                var accessToken = _jwtTokenService.GenerateToken(_options.ActualAccessKey, tokenClaims, DateTime.UtcNow.AddHours(1)).Result;

                var refreshToken = _jwtTokenService.GenerateToken(_options.ActualRefreshKey, tokenClaims, DateTime.UtcNow.AddDays(7)).Result;
                // Cập nhật refresh token cho tài khoản
                user.SetRefreshToken(refreshToken);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Users
                        Set RefreshToken = @RefreshToken
                        Where UserId = @UserId And SiteId = @SiteId
                    ", new
                {
                    RefreshToken = refreshToken,
                    UserId = user.UserId,
                    SiteId = user.SiteId
                }, cancellationToken);

                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}Login-04] Failed to update refresh token for user.", HttpStatusCode.InternalServerError);
                }

                return new AuthorizationOutputDTO
                {
                    AutoId = user.AutoId,
                    UserId = user.UserId,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Thực hiện đăng xuất bằng cách xóa refresh token của tài khoản trong database. Khi người dùng đăng xuất, refresh token sẽ bị vô hiệu hóa, ngăn chặn việc làm mới access token và đảm bảo rằng tài khoản không thể tiếp tục sử dụng các token cũ để truy cập vào hệ thống.
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        public async Task<string> Logout(CancellationToken cancellationToken)
        {
            try
            {
                // Lấy access token từ header
                var accessToken = _jwtTokenService.GetAuthorizattionHeader();    
                // Lấy access key từ cấu hình
                var accessKey = _configuration["BaseRepo:JWT:AccessKey"];
                if (string.IsNullOrEmpty(accessKey))
                {
                    throw new AppException($"[{_errorPrefix}Logout-01] Access key is missing in configuration.", HttpStatusCode.InternalServerError);
                }
                // Lấy thông tin tài khoản từ access token
                List<Claim> claims = await _jwtTokenService.VerifyToken(accessToken, accessKey);
                var userId = _jwtTokenService.GetClaim(claims, ClaimTypes.NameIdentifier).ToString();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new AppException($"[{_errorPrefix}Logout-02] UserId is missing in token claims.", HttpStatusCode.BadRequest);
                }
                // UserId không còn unique toàn hệ thống (chỉ unique trong phạm vi 1 Site) - bắt buộc lấy thêm claim
                // "site_id" (gán ở Login()) để tra đúng tài khoản, tránh Logout nhầm sang tài khoản trùng UserId ở Site khác.
                var siteId = _jwtTokenService.GetClaim(claims, "site_id").ToString();
                if (string.IsNullOrEmpty(siteId))
                {
                    throw new AppException($"[{_errorPrefix}Logout-03] SiteId is missing in token claims.", HttpStatusCode.BadRequest);
                }
                // Lấy tài khoản từ database
                var currentUser = await _dbContext.QueryFirstOrDefaultAsync<User>(@"
                        Select * From tbl_Users
                        Where UserId = @UserId And SiteId = @SiteId
                        ", new
                    {
                        UserId = userId,
                        SiteId = siteId
                    }, cancellationToken);
                if (currentUser == null)
                {
                    throw new AppException($"[{_errorPrefix}Logout-04] User not found.", HttpStatusCode.BadRequest);
                }
                // Xóa refresh token của tài khoản
                await _dbContext.ExecuteAsync(@"
                        Update tbl_Users
                        Set RefreshToken = NULL
                        Where UserId = @UserId And SiteId = @SiteId
                        ", new
                    {
                        UserId = currentUser.UserId,
                        SiteId = currentUser.SiteId
                    }, cancellationToken);
                return currentUser.UserId;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Làm mới access token bằng cách sử dụng refresh token. Khi access token hết hạn, người dùng có thể gửi yêu cầu làm mới token kèm theo refresh token để nhận được access token mới mà không cần phải đăng nhập lại. Hệ thống sẽ kiểm tra tính hợp lệ của refresh token, nếu hợp lệ sẽ tạo mới access token và refresh token, đồng thời cập nhật refresh token mới vào database và trả về cho người dùng. Nếu refresh token không hợp lệ hoặc đã hết hạn, người dùng sẽ được yêu cầu đăng nhập lại để lấy token mới.
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        public async Task<AuthorizationOutputDTO> RenewToken(CancellationToken cancellationToken)
        {
            try
            {
                // Lấy access token từ header
                var accessToken = _jwtTokenService.GetAuthorizattionHeader();

                // Lấy refresh token từ cookie
                string refreshToken = _httpContext.HttpContext.Request.Cookies["RefreshToken"];
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-05] Missing token", HttpStatusCode.BadRequest);
                }

                // Lấy access key và refresh key từ cấu hình
                var accessKey = _configuration["BaseRepo:JWT:AccessKey"];
                if (string.IsNullOrEmpty(accessKey))
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-06] Access key is missing in configuration.", HttpStatusCode.InternalServerError);
                }
                var refreshKey = _configuration["BaseRepo:JWT:RefreshKey"];
                if (string.IsNullOrEmpty(refreshKey))
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-07] RefreshKey key is missing in configuration.", HttpStatusCode.InternalServerError);
                }
                // Kiểm tra access token đã hết hạn chưa
                if (!_jwtTokenService.IsTokenExpired(accessToken, accessKey))
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-08] Access token is not expired.");
                }
                // Lấy thông tin tài khoản từ refresh token
                var currentUser = await _dbContext.QueryFirstOrDefaultAsync<User>(@"
                        Select * From tbl_Users
                        Where RefreshToken = @RefreshToken
                        ", new
                    {
                        RefreshToken = refreshToken
                    }, cancellationToken);
                if (currentUser == null)
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-09] Refresh token is not valid.");
                }

                // Kiểm tra refresh token đã hết hạn chưa
                if (_jwtTokenService.IsTokenExpired(refreshToken, refreshKey))
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-10] Refresh token has been expired. Please signin again.");
                }

                // Kiểm tra refresh token có khớp với tài khoản không
                var claims = await _jwtTokenService.VerifyToken(refreshToken, refreshKey);
                // Tạo mới access token và refresh token
                accessToken = await _jwtTokenService.GenerateToken(accessKey, claims, DateTime.UtcNow.AddHours(1));
                refreshToken = await _jwtTokenService.GenerateToken(refreshKey, claims, DateTime.UtcNow.AddDays(7));

                // Cập nhật refresh token cho tài khoản - lọc thêm SiteId (không chỉ UserId): UserId không còn
                // unique toàn cục, thiếu điều kiện này có thể vô tình cập nhật RefreshToken của MỌI User trùng
                // UserId ở các Site khác (UPDATE nhiều dòng cùng lúc) dù currentUser đã được xác định chính xác
                // ở trên qua RefreshToken (vốn unique).
                await _dbContext.ExecuteAsync(@"
                        Update tbl_Users
                        Set RefreshToken = @RefreshToken
                        Where UserId = @UserId And SiteId = @SiteId
                        ", new
                    {
                        RefreshToken = refreshToken,
                        UserId = currentUser.UserId,
                        SiteId = currentUser.SiteId
                    }, cancellationToken);

                return new AuthorizationOutputDTO
                {
                    AutoId = currentUser.AutoId,
                    UserId = currentUser.UserId,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Xác thực token bằng cách kiểm tra tính hợp lệ của token, bao gồm việc xác minh chữ ký, kiểm tra thời gian hết hạn và các thông tin liên quan khác. Nếu token hợp lệ, phương thức sẽ trả về danh sách các claim có trong token, cho phép hệ thống sử dụng các thông tin này để xác định quyền truy cập và thực hiện các hành động phù hợp. Nếu token không hợp lệ hoặc đã hết hạn, phương thức sẽ ném ra các ngoại lệ tương ứng để xử lý lỗi và yêu cầu người dùng đăng nhập lại nếu cần thiết.
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="key">Key</param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<List<Claim>> VerifyToken(string token, string key)
        {
            try
            {
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _options.ActualIssuer,
                    ValidateAudience = true,
                    ValidAudience = _options.ActualAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    TokenDecryptionKey = null
                };

                _jwtSecurityTokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var jwt = (JwtSecurityToken)validatedToken;
                return jwt.Claims.ToList();

            }
            catch (SecurityTokenMalformedException)
            {
                throw new AppException("Invalid token.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentException)
            {
                throw new AppException("Invalid token.", HttpStatusCode.BadRequest);
            }
            catch (SecurityTokenException ex)
            {
                switch (ex)
                {
                    case SecurityTokenExpiredException:
                        throw new AppException("Token has been expired.", HttpStatusCode.BadRequest);
                    case SecurityTokenValidationException:
                        throw new AppException("Token is not valid.", HttpStatusCode.BadRequest);
                    default:
                        throw new AppException(ex.Message, HttpStatusCode.BadRequest);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Xác định SiteId sẽ dùng để tra tài khoản khi Login. Nếu client đã truyền SiteId thì dùng thẳng (bản
        /// thuê trọn bộ, nhiều Site chung 1 cụm hạ tầng - bắt buộc chọn đúng Site muốn đăng nhập). Nếu không truyền,
        /// chỉ tự suy ra được khi toàn hệ thống có đúng 1 Site đang active (bản deploy on-prem/silo cho khách có
        /// máy chủ riêng) - nhiều hơn 1 Site mà không truyền SiteId thì không có cách nào suy đoán an toàn UserId
        /// thuộc Site nào, phải yêu cầu client truyền lên.
        /// </summary>
        private async Task<string> ResolveSiteId(string? inputSiteId, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(inputSiteId))
            {
                return inputSiteId.Trim();
            }

            var activeSiteIds = (await _dbContext.QueryAsync<string>(@"
                    Select SiteId From tbl_Site Where Status = 1
                ", cancellationToken: cancellationToken)).ToList();

            if (activeSiteIds.Count == 0)
            {
                throw new AppException($"[{_errorPrefix}Login-ResolveSiteId-01] System has no active Site configured.", HttpStatusCode.InternalServerError);
            }
            if (activeSiteIds.Count > 1)
            {
                throw new AppException($"[{_errorPrefix}Login-ResolveSiteId-02] SiteId is required because the system has more than one active Site.", HttpStatusCode.BadRequest);
            }

            return activeSiteIds[0];
        }

        private sealed class RoleAssignment
        {
            public string RoleName { get; set; } = string.Empty;
            public string SiteId { get; set; } = string.Empty;
        }
        #endregion
    }
}
