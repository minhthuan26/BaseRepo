using AutoMapper;
using CommonLibs.DTOs;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Services;
using CommonLibs.Utils;
using Dapper;
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
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gateway.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<UserRepository>, IUserRepository
    {
        private readonly GatewayDbContext _dbContext;
        private readonly string _errorPrefix = "User_Repo_";
        public UserRepository(IMapper mapper, ILogger<UserRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, GatewayDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
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
                if (string.IsNullOrEmpty(_options.ActualAccessKey))
                {
                    throw new AppException($"[{_errorPrefix}Login-03] Access key is missing in configuration.", HttpStatusCode.InternalServerError);
                }
                string encryptedPassword = EncryptionUtils.EncryptData(input.Password, _options.ApplicationName, _options.ApplicationId);
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

        public async Task<string> DeleteUser(string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteUser-01] UserId is required.", HttpStatusCode.BadRequest);
                }
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_Users
                        Where UserId = @UserId
                        ", new
                {
                    UserId = userId
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}DeleteUser-02] User with UserId '{userId}' not found.", HttpStatusCode.NotFound);
                }
                return userId;
            }
            catch
            {
                throw;
            }
        }

        public Task<string> ExportUsers(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseFilterOutput<UserOutputDTO>> GetAllUsers(UserFilterDTO filter, CancellationToken cancellationToken)
        {
            try
            {
                var whereBuilder = new StringBuilder(" WHERE 1=1 ");
                var parameters = new DynamicParameters();
                if (filter.Status.HasValue)
                {
                    whereBuilder.Append(" AND [Status] = @Status ");
                    parameters.Add("Status", filter.Status.Value);
                }
                if (!string.IsNullOrWhiteSpace(filter.Email))
                {
                    whereBuilder.Append(" AND Email = @Email ");
                    parameters.Add("Email", filter.Email.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filter.UserName))
                {
                    whereBuilder.Append(" AND UserName like @UserName ");
                    parameters.Add("UserName", $"%{filter.UserName.Trim()}%", System.Data.DbType.String);
                }
                if (!string.IsNullOrWhiteSpace(filter.QuickCode))
                {
                    whereBuilder.Append(" AND QuickCode = @QuickCode ");
                    parameters.Add("QuickCode", filter.QuickCode.Trim());
                }
                if (filter.Priority.HasValue)
                {
                    whereBuilder.Append(" AND Priority = @Priority ");
                    parameters.Add("Priority", filter.Priority.Value);
                }
                if (filter.FromDate.HasValue)
                {
                    whereBuilder.Append(" AND CreatedDate >= @FromDate ");
                    parameters.Add("FromDate", filter.FromDate.Value);
                }
                if (filter.ToDate.HasValue)
                {
                    whereBuilder.Append(" AND CreatedDate <= @ToDate ");
                    parameters.Add("ToDate", filter.ToDate.Value);
                }
                string countSql = $"SELECT COUNT(1) FROM tbl_Users {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<UserOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<UserOutputDTO>()
                };
                if (totalCount == 0)
                {
                    return filterOutput;
                }

                string orderByClause = filter.SortBy switch
                {
                    Sort.AutoIdDes => " ORDER BY AutoId DESC ",
                    _ => " ORDER BY AutoId ASC "
                };

                string querySql = $"SELECT * FROM tbl_Users {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var users = await _dbContext.QueryAsync<User>(querySql, parameters, cancellationToken);

                filterOutput.Records = users.Select(u =>
                {
                    u.SetRefreshToken(null);
                    return _mapper.Map<UserOutputDTO>(u);
                }).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<UserOutputDTO> GetUserById(string userId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _dbContext.QueryFirstOrDefaultAsync<User>(
                    @"Select * From tbl_Users
                    Where UserId = @UserId",
                    new { UserId = userId },
                    cancellationToken);
                if (user == null)
                {
                    throw new AppException($"[{_errorPrefix}GetUserById-01] User with UserId '{userId}' not found.", HttpStatusCode.NotFound);
                }
                user.SetRefreshToken(null);
                return _mapper.Map<UserOutputDTO>(user);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<UserOutputDTO>> ImportUsers(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var users = await FileHelper.ReadFile<UserInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<UserInputDTO>());

            if (!users.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempUsers
                    (
                        UserId NVARCHAR(100),
                        UserName NVARCHAR(255),
                        Password VARCHAR(255),
                        Signature VARBINARY(MAX),
                        Email NVARCHAR(255),
                        QuickCode NVARCHAR(100),
                        Status INT,
                        Priority INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    users,
                    "#TempUsers",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_Users AS T
                    USING #TempUsers AS S
                    ON T.UserId = S.UserId

                    WHEN MATCHED THEN
                        UPDATE SET
                            T.UserName = S.UserName,
                            T.Email = S.Email,
                            T.Password = S.Password,
                            T.Signature = S.Signature,
                            T.QuickCode = S.QuickCode,
                            T.Status = S.Status,
                            T.Priority = S.Priority,
                            T.UpdatedDate = GETUTCDATE(),
                            T.UpdatedBy = @UpdatedBy, 
                            T.DeviceName = @DeviceName, 
                            T.DeviceId = @DeviceId

                    WHEN NOT MATCHED THEN
                        INSERT
                        (
                            UserId,
                            UserName,
                            Email,
                            Password,
                            Signature,
                            QuickCode,
                            Status,
                            Priority,
                            CreatedDate,
                            CreatedBy, 
                            DeviceName, 
                            DeviceId
                        )
                        VALUES
                        (
                            S.UserId,
                            S.UserName,
                            S.Email,
                            S.Password,
                            S.Signature,
                            S.QuickCode,
                            S.Status,
                            S.Priority,
                            GETUTCDATE(),
                            @CreatedBy,
                            @DeviceName,
                            @DeviceId,
                        );
                    ", new
                    {
                        UpdatedBy = _auditContext.UserName,
                        CreatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId,
                    },
                    cancellationToken: cancellationToken);

                return _mapper.Map<List<UserOutputDTO>>(users);
            }, cancellationToken);
        }

        public async Task<UserOutputDTO> UpdateUser(UserInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}UpdateUser-01] Input is null.", HttpStatusCode.BadRequest);
                }
                
                if (string.IsNullOrWhiteSpace(input.UserId))
                {
                    throw new AppException($"[{_errorPrefix}UpdateUser-02] UserId is required.", HttpStatusCode.BadRequest);
                }
                
                if (!await CheckUserExists(input.UserId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}UpdateUser-03] User with UserId '{input.UserId}' not found.", HttpStatusCode.NotFound);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

                string updateQuery = @"
                        Update tbl_Users
                        Set UserName = @UserName,
                            Priority = @Priority,
                            Signature = @Signature,
                            Email = @Email,
                            QuickCode = @QuickCode,
                            Status = @Status,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId";
                updateQuery += " Where UserId = @UserId";

                int rowsAffected = await _dbContext.ExecuteAsync(updateQuery, new
                {
                    UserId = input.UserId,
                    UserName = input.UserName,
                    Priority = input.Priority,
                    Signature = input.Signature != null ? Convert.FromBase64String(input.Signature) : null,
                    Email = input.Email,
                    QuickCode = input.QuickCode,
                    Status = input.Status.HasValue ? (int)input.Status.Value : 1,
                    UpdatedDate = DateTime.UtcNow,
                    UpdatedBy = input.UpdatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                }, cancellationToken);

                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}UpdateUser-05] Failed to update user.", HttpStatusCode.InternalServerError);
                }

                return _mapper.Map<UserOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }
        public async Task<UserOutputDTO> ChangePassword(ChangePasswordInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                    throw new AppException($"[{_errorPrefix}ChangePassword-01] Input is null.", HttpStatusCode.BadRequest);

                if (string.IsNullOrWhiteSpace(input.UserId))
                    throw new AppException($"[{_errorPrefix}ChangePassword-02] UserId is required.", HttpStatusCode.BadRequest);

                if (string.IsNullOrWhiteSpace(input.OldPassword))
                    throw new AppException($"[{_errorPrefix}ChangePassword-03] OldPassword is required.", HttpStatusCode.BadRequest);

                if (string.IsNullOrWhiteSpace(input.NewPassword))
                    throw new AppException($"[{_errorPrefix}ChangePassword-04] NewPassword is required.", HttpStatusCode.BadRequest);

                if (input.NewPassword != input.ConfirmNewPassword)
                    throw new AppException($"[{_errorPrefix}ChangePassword-05] NewPassword and ConfirmNewPassword do not match.", HttpStatusCode.BadRequest);

                var user = await _dbContext.QueryFirstOrDefaultAsync<User>(
                    @"Select * From tbl_Users Where UserId = @UserId",
                    new { UserId = input.UserId },
                    cancellationToken);

                if (user == null)
                    throw new AppException($"[{_errorPrefix}ChangePassword-06] User not found.", HttpStatusCode.NotFound);

                if (string.IsNullOrEmpty(_options.ActualAccessKey))
                    throw new AppException($"[{_errorPrefix}ChangePassword-07] Access key is missing in configuration.", HttpStatusCode.InternalServerError);

                string encryptedOldPassword = EncryptionUtils.EncryptData(input.OldPassword, _options.ApplicationName, _options.ApplicationId);
                
                if (user.Password != encryptedOldPassword)
                    throw new AppException($"[{_errorPrefix}ChangePassword-08] OldPassword is incorrect.", HttpStatusCode.BadRequest);

                string encryptedNewPassword = EncryptionUtils.EncryptData(input.NewPassword, _options.ApplicationName, _options.ApplicationId);

                int rowsAffected = await _dbContext.ExecuteAsync(@"
                    Update tbl_Users
                    Set Password = @Password,
                        UpdatedDate = @UpdatedDate,
                        UpdatedBy = @UpdatedBy,
                        DeviceName = @DeviceName,
                        DeviceId = @DeviceId
                    Where UserId = @UserId",
                    new
                    {
                        Password = encryptedNewPassword,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId,
                        UserId = input.UserId
                    }, cancellationToken);

                if (rowsAffected <= 0)
                    throw new AppException($"[{_errorPrefix}ChangePassword-09] Failed to change password.", HttpStatusCode.InternalServerError);

                return _mapper.Map<UserOutputDTO>(user);
            }
            catch
            {
                throw;
            }
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
