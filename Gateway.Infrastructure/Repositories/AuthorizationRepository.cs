using AutoMapper;
using CommonLibs.Options;
using CommonLibs.Repositories;
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
        private readonly string _errorPrefix = "Authorization_Repo_";

        public AuthorizationRepository(IMapper mapper,
            ILogger<AuthorizationRepository> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContext,
            JwtSecurityTokenHandler jwtSecurityTokenHandler,
            GatewayDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext) : base(mapper, logger, configuration, httpContext, options, auditContext)
        {
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
            _dbContext = dbContext;
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

                // Xác thực tài khoản
                var encryptedPassword = EncryptionUtils.EncryptData(input.Password, _options.ApplicationName, _options.ApplicationId);
                var user = await _dbContext.QueryFirstOrDefaultAsync<User>(@"
                        Select * From tbl_Users
                        Where UserId = @UserId And Password = @Password
                    ", new
                    {
                        UserId = input.UserId,
                        Password = EncryptionUtils.EncryptData(input.Password, _options.ApplicationName, _options.ApplicationId)
            }, 
                    cancellationToken);
                if (user == null)
                {
                    throw new AppException($"[{_errorPrefix}Login-03] Invalid UserId or Password.", HttpStatusCode.Unauthorized);
                }
                // Tạo access token và refresh token
                var accessToken = GenerateToken(_options.ActualAccessKey, new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.UserId),
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.Role, "User")
                }, DateTime.UtcNow.AddHours(1)).Result;

                var refreshToken = GenerateToken(_options.ActualRefreshKey, new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.UserId),
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.Role, "User")
                }, DateTime.UtcNow.AddDays(7)).Result;
                // Cập nhật refresh token cho tài khoản
                user.SetRefreshToken(refreshToken);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Users
                        Set RefreshToken = @RefreshToken
                        Where UserId = @UserId
                    ", new
                {
                    RefreshToken = refreshToken,
                    UserId = user.UserId
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
                var accessToken = GetAuthorizattionHeader();
                // Lấy access key từ cấu hình
                var accessKey = _configuration["BaseRepo:JWT:AccessKey"];
                if (string.IsNullOrEmpty(accessKey))
                {
                    throw new AppException($"[{_errorPrefix}Logout-01] Access key is missing in configuration.", HttpStatusCode.InternalServerError);
                }
                // Lấy thông tin tài khoản từ access token
                List<Claim> claims = await VerifyToken(accessToken, accessKey);
                var userId = GetClaim(claims, ClaimTypes.NameIdentifier).ToString();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new AppException($"[{_errorPrefix}Logout-02] UserId is missing in token claims.", HttpStatusCode.BadRequest);
                }
                // Lấy tài khoản từ database
                var currentUser = await _dbContext.QueryFirstOrDefaultAsync<User>(@"
                        Select * From tbl_Users
                        Where UserId = @UserId
                        ", new
                    {
                        UserId = userId
                    }, cancellationToken);
                if (currentUser == null)
                {
                    throw new AppException($"[{_errorPrefix}Logout-03] User not found.", HttpStatusCode.BadRequest);
                }
                // Xóa refresh token của tài khoản
                await _dbContext.ExecuteAsync(@"
                        Update tbl_Users
                        Set RefreshToken = NULL
                        Where UserId = @UserId
                        ", new
                    {
                        UserId = currentUser.UserId
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
                var accessToken = GetAuthorizattionHeader();

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
                if (!IsTokenExpired(accessToken, accessKey))
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
                if (IsTokenExpired(refreshToken, refreshKey))
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-10] Refresh token has been expired. Please signin again.");
                }

                // Kiểm tra refresh token có khớp với tài khoản không
                var claims = await VerifyToken(refreshToken, refreshKey);
                // Tạo mới access token và refresh token
                accessToken = await GenerateToken(accessKey, claims, DateTime.UtcNow.AddHours(1));
                refreshToken = await GenerateToken(refreshKey, claims, DateTime.UtcNow.AddDays(7));

                // Cập nhật refresh token cho tài khoản
                await _dbContext.ExecuteAsync(@"
                        Update tbl_Users
                        Set RefreshToken = @RefreshToken
                        Where UserId = @UserId
                        ", new
                    {
                        RefreshToken = refreshToken,
                        UserId = currentUser.UserId
                    }, cancellationToken);

                // Lưu refresh token vào cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Path = "/"
                };
                _httpContext.HttpContext?.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

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
        private string GetAuthorizattionHeader()
        {
            try
            {
                // Kiểm tra header và lấy Authorization
                var httpContext = _httpContext.HttpContext ?? throw new AppException($"[{_errorPrefix}RenewToken-01] Http context is unavailable.", HttpStatusCode.Unauthorized);
                if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-02] Authorization header is missing.", HttpStatusCode.Unauthorized);
                }
                // Kiểm tra Bearer token hợp lệ
                var authorization = authHeader.ToString();
                if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-03] Invalid authorization scheme.", HttpStatusCode.Unauthorized);
                }
                // Tách Bearer và kiểm tra hợp lệ
                var accessToken = authorization["Bearer ".Length..].Trim();
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-04] Access token is missing.", HttpStatusCode.Unauthorized);
                }
                return accessToken;
            }
            catch
            {
                throw;
            }
        }

        private async Task<string> GenerateToken(string key, List<Claim> claims, DateTime expires)
        {
            try
            {
                var signingCredential = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha512Signature
                );

                var tokenDescription = new JwtSecurityToken(
                    issuer: _options.ActualIssuer,
                    audience:_options.ActualAudience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: signingCredential
                );

                var token = _jwtSecurityTokenHandler.WriteToken(tokenDescription);
                if (token == null)
                {
                    throw new AppException($"[{_errorPrefix}GenerateToken-01] Failed to generate JWT token.", HttpStatusCode.InternalServerError);
                }
                return token;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool IsTokenExpired(string token, string key)
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
                    IssuerSigningKeys = new List<SecurityKey> { signingKey },
                    ValidateLifetime = true
                };

                _jwtSecurityTokenHandler.ValidateToken(token, validationParameters, out _);

                return false;
            }
            catch (SecurityTokenException ex)
            {
                switch (ex)
                {
                    case SecurityTokenExpiredException:
                        return true;
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

        public object GetClaim(string type)
        {
            try
            {
                var claims = (List<Claim>)_httpContext.HttpContext.Items["claims"];
                if (claims == null || claims.Count == 0)
                {
                    throw new AppException("Missing token claims", HttpStatusCode.BadRequest);
                }
                var claimsIdentity = new ClaimsIdentity(claims);
                var claim = claimsIdentity.Claims.FirstOrDefault(claim => claim.Type == type)?.Value;
                if (claim == null)
                {
                    throw new AppException($"Missing claim type: {type}", HttpStatusCode.BadRequest);
                }
                return claim;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public object GetClaim(List<Claim> claims, string type)
        {
            try
            {
                var claimsIdentity = new ClaimsIdentity(claims);
                var claim = claimsIdentity.Claims.FirstOrDefault(claim => claim.Type == type)?.Value;
                if (claim == null)
                {
                    throw new AppException($"Missing claim type: {type}", HttpStatusCode.BadRequest);
                }
                return claim;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
