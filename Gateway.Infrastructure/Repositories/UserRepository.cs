using AutoMapper;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Utils;
using Gateway.Application.Interfaces;
using Gateway.Domain.DTOs;
using Gateway.Domain.Entities;
using Gateway.Infrastructure.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<UserRepository>, IUserRepository
    {
        private readonly GatewayDbContext _dbContext;
        private readonly string _errorPrefix = "User_Repo_";
        public UserRepository(IMapper mapper, ILogger<UserRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, GatewayDbContext dbContext, IOptions<ApplicationOptions> options, IAuditContext auditContext) : base(mapper, logger, configuration, httpContext, options, auditContext)
        {
            _dbContext = dbContext;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<UserOutputDTO> CreateUser(UserInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                // Kiểm tra input
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateUser-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.UserId))
                {
                    throw new AppException($"[{_errorPrefix}CreateUser-02] UserId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.UserName))
                {
                    throw new AppException($"[{_errorPrefix}CreateUser-03] UserName is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.Password))
                {
                    throw new AppException($"[{_errorPrefix}CreateUser-04] Password is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckUserExists(input.UserId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateUser-05] User with UserId '{input.UserId}' already exists.", HttpStatusCode.Conflict);
                }
                if (string.IsNullOrEmpty(_options.ActualAccessToken))
                {
                    throw new AppException($"[{_errorPrefix}Login-03] Access key is missing in configuration.", HttpStatusCode.InternalServerError);
                }
                string encryptedPassword = Cypher.AesCypher.Encrypt(input.Password, _options.ActualAccessTokenByte).CipherText;
                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_Users (UserId, UserName, Password, Priority, Signature, Email, QuickCode, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@UserId, @UserName, @Password, @Priority, @Signature, @Email, @QuickCode, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    UserId = input.UserId,
                    UserName = input.UserName,
                    Password = encryptedPassword,
                    Priority = input.Priority,
                    Signature = input.Signature != null ? Convert.FromBase64String(input.Signature) : null,
                    Email = input.Email,
                    QuickCode = input.QuickCode,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateUser-06] Failed to create user.", HttpStatusCode.InternalServerError);
                }
                return _mapper.Map<UserOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public Task<int> DeleteUser(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserOutputDTO>> GetAllUsers(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserOutputDTO> GetUserById(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserOutputDTO>> ImportUsers(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task<UserOutputDTO> UpdateUser(UserInputDTO input, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Helpers
        private async Task<bool> CheckUserExists(string userId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _dbContext.QueryFirstOrDefaultAsync<User>(
                    @"Select * From tbl_Users
                    Where UserId = @UserId",
                    new { UserId = userId },
                    cancellationToken);

                return user != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        #endregion
    }
}
