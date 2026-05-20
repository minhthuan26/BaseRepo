using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using CommonLibs.Repositories;
using CommonLibs.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Gateway.Infrastructure.DBContext;
using Gateway.Application.Interfaces;
using Gateway.Domain.DTOs;

namespace Gateway.Infrastructure.Repositories
{
    public class GatewayRepository : BaseRepository<GatewayRepository>, IGatewayRepository
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly GatewayDbContext _dbContext;
        private readonly string _errorPrefix = "Gateway_Repo_";

        public GatewayRepository(IMapper mapper,
            ILogger<GatewayRepository> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContext,
            JwtSecurityTokenHandler jwtSecurityTokenHandler,
            GatewayDbContext dbContext) : base(mapper, logger, configuration, httpContext)
        {
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
            _dbContext = dbContext;
        }

        #region Stored Procedure
        private async Task<DataTable> GetAccountByRefreshTokenAsync(string refreshToken)
        {
            return await ExecuteProcAsync(_dbContext, "dbo.Proc_Account_GetByRefreshToken",
            new[] { new SqlParameter("@RefreshToken", refreshToken) });
        }
        #endregion

        #region Repository Methods
        public async Task<AuthorizationOutputDTO> Login(AuthorizationInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                // Kiểm tra input
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}Login-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrEmpty(input.AccountName) || string.IsNullOrEmpty(input.Password))
                {
                    throw new AppException($"[{_errorPrefix}Login-02] AccountName or Password is null or empty.", HttpStatusCode.BadRequest);
                }
                // Lấy access key và refresh key từ cấu hình
                var accessKey = _configuration["Mitalab:JWT:AccessKey"];
                if (string.IsNullOrEmpty(accessKey))
                {
                    throw new AppException($"[{_errorPrefix}Login-03] Access key is missing in configuration.", HttpStatusCode.InternalServerError);
                }
                var refreshKey = _configuration["Mitalab:JWT:RefreshKey"];
                if (string.IsNullOrEmpty(refreshKey))
                {
                    throw new AppException($"[{_errorPrefix}Login-04] RefreshKey key is missing in configuration.", HttpStatusCode.InternalServerError);
                }
                // Xác thực tài khoản
                var account = _dbContext.Accounts.FirstOrDefault(a => a.AccountName == input.AccountName && a.Password == input.Password);
                if (account == null)
                {
                    throw new AppException($"[{_errorPrefix}Login-05] Invalid AccountName or Password.", HttpStatusCode.Unauthorized);
                }
                // Tạo access token và refresh token
                var accessToken = GenerateToken(accessKey, new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.Id),
                    new Claim(ClaimTypes.Name, account.AccountName)
                }, DateTime.UtcNow.AddHours(10)).Result;

                var refreshToken = GenerateToken(refreshKey, new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.Id),
                    new Claim(ClaimTypes.Name, account.AccountName)
                }, DateTime.UtcNow.AddDays(7)).Result;
                // Cập nhật refresh token cho tài khoản
                account.SetRefreshToken(refreshToken);
                _dbContext.Accounts.Update(account);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new AuthorizationOutputDTO
                {
                    Id = account.Id,
                    AccountName = account.AccountName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> Logout(CancellationToken cancellationToken)
        {
            try
            {
                // Lấy access token từ header
                var accessToken = GetAuthorizattionHeader();
                // Lấy access key từ cấu hình
                var accessKey = _configuration["Mitalab:JWT:AccessKey"];
                if (string.IsNullOrEmpty(accessKey))
                {
                    throw new AppException($"[{_errorPrefix}Logout-01] Access key is missing in configuration.", HttpStatusCode.InternalServerError);
                }
                // Lấy thông tin tài khoản từ access token
                List<Claim> claims = await VerifyToken(accessToken, accessKey);
                var accountId = GetClaim(claims, ClaimTypes.NameIdentifier).ToString();
                if (string.IsNullOrEmpty(accountId))
                {
                    throw new AppException($"[{_errorPrefix}Logout-02] AccountId is missing in token claims.", HttpStatusCode.BadRequest);
                }
                // Lấy tài khoản từ database
                var currentAccount = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
                if (currentAccount == null)
                {
                    throw new AppException($"[{_errorPrefix}Logout-03] Account not found.", HttpStatusCode.BadRequest);
                }
                // Xóa refresh token của tài khoản
                currentAccount.SetRefreshToken(null);
                _dbContext.Accounts.Update(currentAccount);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return currentAccount.Id;
            }
            catch
            {
                throw;
            }
        }

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
                var accessKey = _configuration["Mitalab:JWT:AccessKey"];
                if (string.IsNullOrEmpty(accessKey))
                {
                    throw new AppException($"[{_errorPrefix}RenewToken-06] Access key is missing in configuration.", HttpStatusCode.InternalServerError);
                }
                var refreshKey = _configuration["Mitalab:JWT:RefreshKey"];
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
                var currentAccount = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.RefreshToken == refreshToken);
                if (currentAccount == null)
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
                currentAccount.SetRefreshToken(refreshToken);
                _dbContext.Accounts.Update(currentAccount);
                await _dbContext.SaveChangesAsync(cancellationToken);

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
                    Id = currentAccount.Id,
                    AccountName = currentAccount.AccountName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

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

        public async Task<List<Claim>> VerifyToken(string token, string key)
        {
            try
            {
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Mitalab:JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Mitalab:JWT:Audience"],
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
        private async Task<string> GenerateToken(string key, List<Claim> claims, DateTime expires)
        {
            try
            {
                var signingCredential = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha512Signature
                );

                var tokenDescription = new JwtSecurityToken(
                    issuer: _configuration["Mitalab:JWT:Issuer"],
                    audience: _configuration["Mitalab:JWT:Audience"],
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
                    ValidIssuer = _configuration["Mitalab:JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Mitalab:JWT:Audience"],
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
