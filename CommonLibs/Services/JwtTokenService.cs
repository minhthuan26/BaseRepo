using CommonLibs.Options;
using CommonLibs.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Services
{
    public interface IJwtTokenService
    {
        Task<List<Claim>> VerifyToken(string token, string key);
        string GetAuthorizattionHeader();
        Task<string> GenerateToken(string key, List<Claim> claims, DateTime expires);
        bool IsTokenExpired(string token, string key);
        object GetClaim(string type);
        object GetClaim(List<Claim> claims, string type);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly IHttpContextAccessor _httpContext;
        private readonly AppOptions _options;
        private readonly string _errorPrefix;

        public JwtTokenService(JwtSecurityTokenHandler jwtSecurityTokenHandler, IHttpContextAccessor httpContext, IOptions<AppOptions> options, string errorPrefix)
        {
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
            _httpContext = httpContext;
            _options = options.Value;
            _errorPrefix = errorPrefix;
        }

        #region Helpers
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

        public string GetAuthorizattionHeader()
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

        public async Task<string> GenerateToken(string key, List<Claim> claims, DateTime expires)
        {
            try
            {
                var signingCredential = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha512Signature
                );

                var tokenDescription = new JwtSecurityToken(
                    issuer: _options.ActualIssuer,
                    audience: _options.ActualAudience,
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

        public bool IsTokenExpired(string token, string key)
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
