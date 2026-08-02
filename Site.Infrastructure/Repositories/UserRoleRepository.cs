using AutoMapper;
using CommonLibs.DTOs;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Services;
using CommonLibs.Utils;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Site.Application.Interfaces;
using Site.Domain.DTOs;
using Site.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RoleEntity = Site.Domain.Entities.Role;
using UserRoleEntity = Site.Domain.Entities.UserRole;

namespace Site.Infrastructure.Repositories
{
    public class UserRoleRepository : BaseRepository<UserRoleRepository>, IUserRoleRepository
    {
        private readonly SiteDbContext _dbContext;
        private readonly IRedisService _redisService;
        private readonly string _errorPrefix = "UserRole_Repo_";

        public UserRoleRepository(IMapper mapper, ILogger<UserRoleRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService, IRedisService redisService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
            _redisService = redisService;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<UserRoleOutputDTO> CreateUserRole(UserRoleInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.UserId))
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-02] UserId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.RoleId))
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-03] RoleId is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckUserRoleExists(input.UserId, input.RoleId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-04] UserRole with UserId '{input.UserId}' and RoleId '{input.RoleId}' already exists.", HttpStatusCode.Conflict);
                }

                // Role phải thuộc 1 Site cụ thể, và user chỉ được gán Role nếu đã là thành viên của Site đó (tbl_User_Site).
                var roleSiteId = await _dbContext.QueryFirstOrDefaultAsync<string>(@"
                    Select SiteId From tbl_Roles Where RoleId = @RoleId And Status = 1",
                    new { RoleId = input.RoleId },
                    cancellationToken);
                if (roleSiteId == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-05] Role with RoleId '{input.RoleId}' not found.", HttpStatusCode.NotFound);
                }

                var isUserSiteMember = await _dbContext.QueryFirstOrDefaultAsync<int?>(@"
                    Select Top 1 1 From tbl_User_Site Where UserId = @UserId And SiteId = @SiteId And Status = 1",
                    new { UserId = input.UserId, SiteId = roleSiteId },
                    cancellationToken);
                if (isUserSiteMember == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-06] User '{input.UserId}' must belong to the site '{roleSiteId}' (via UserSite) before being assigned this role.", HttpStatusCode.BadRequest);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_User_Role (UserId, RoleId, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@UserId, @RoleId, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    UserId = input.UserId,
                    RoleId = input.RoleId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-07] Failed to create user role.", HttpStatusCode.InternalServerError);
                }

                // Quyền của user vừa thay đổi (thêm 1 Role) -> xoá cache Redis để lần request kế tiếp nạp lại dữ liệu mới.
                await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(input.UserId));

                return _mapper.Map<UserRoleOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<UserRoleOutputDTO> GetUserRoleById(string userId, string roleId, CancellationToken cancellationToken)
        {
            try
            {
                var userRole = await _dbContext.QueryFirstOrDefaultAsync<UserRoleEntity>(
                    @"Select * From tbl_User_Role
                    Where UserId = @UserId And RoleId = @RoleId",
                    new { UserId = userId, RoleId = roleId },
                    cancellationToken);
                if (userRole == null)
                {
                    throw new AppException($"[{_errorPrefix}GetUserRoleById-01] UserRole with UserId '{userId}' and RoleId '{roleId}' not found.", HttpStatusCode.NotFound);
                }
                return _mapper.Map<UserRoleOutputDTO>(userRole);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<UserRoleOutputDTO>> GetAllUserRoles(UserRoleFilterDTO filter, CancellationToken cancellationToken)
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
                if (!string.IsNullOrWhiteSpace(filter.UserId))
                {
                    whereBuilder.Append(" AND UserId = @UserId ");
                    parameters.Add("UserId", filter.UserId.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filter.RoleId))
                {
                    whereBuilder.Append(" AND RoleId = @RoleId ");
                    parameters.Add("RoleId", filter.RoleId.Trim());
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
                string countSql = $"SELECT COUNT(1) FROM tbl_User_Role {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<UserRoleOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<UserRoleOutputDTO>()
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

                string querySql = $"SELECT * FROM tbl_User_Role {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var userRoles = await _dbContext.QueryAsync<UserRoleEntity>(querySql, parameters, cancellationToken);

                filterOutput.Records = userRoles.Select(ur => _mapper.Map<UserRoleOutputDTO>(ur)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteUserRole(DeleteUserRoleInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.UserId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteUserRole-01] UserId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.RoleId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteUserRole-02] RoleId is required.", HttpStatusCode.BadRequest);
                }
                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_User_Role
                        Set Status = -1,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where UserId = @UserId And RoleId = @RoleId
                        ", new
                    {
                        UserId = input.UserId,
                        RoleId = input.RoleId,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId
                    },
                        cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteUserRole-03] UserRole with UserId '{input.UserId}' and RoleId '{input.RoleId}' not found.", HttpStatusCode.NotFound);
                    }
                    await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(input.UserId));
                    return $"{input.UserId}/{input.RoleId}";
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_User_Role
                        Where UserId = @UserId And RoleId = @RoleId
                        ", new
                    {
                        UserId = input.UserId,
                        RoleId = input.RoleId
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteUserRole-03] UserRole with UserId '{input.UserId}' and RoleId '{input.RoleId}' not found.", HttpStatusCode.NotFound);
                    }
                    await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(input.UserId));
                    return $"{input.UserId}/{input.RoleId}";
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<UserRoleOutputDTO>> ImportUserRoles(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var userRoles = await FileHelper.ReadFile<UserRoleInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<UserRoleInputDTO>());

            if (!userRoles.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempUserRoles
                    (
                        UserId NVARCHAR(20),
                        RoleId NVARCHAR(50),
                        Status INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    userRoles,
                    "#TempUserRoles",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_User_Role AS T
                    USING #TempUserRoles AS S
                    ON T.UserId = S.UserId AND T.RoleId = S.RoleId

                    WHEN MATCHED THEN
                        UPDATE SET
                            T.Status = S.Status,
                            T.UpdatedDate = GETUTCDATE(),
                            T.UpdatedBy = @UpdatedBy,
                            T.DeviceName = @DeviceName,
                            T.DeviceId = @DeviceId

                    WHEN NOT MATCHED THEN
                        INSERT
                        (
                            UserId,
                            RoleId,
                            Status,
                            CreatedDate,
                            CreatedBy,
                            DeviceName,
                            DeviceId
                        )
                        VALUES
                        (
                            S.UserId,
                            S.RoleId,
                            S.Status,
                            GETUTCDATE(),
                            @CreatedBy,
                            @DeviceName,
                            @DeviceId
                        );
                    ", new
                {
                    UpdatedBy = _auditContext.UserName,
                    CreatedBy = _auditContext.UserName,
                    DeviceName = _auditContext.DeviceName,
                    DeviceId = _auditContext.DeviceId,
                },
                    cancellationToken: cancellationToken);

                // Quyền của toàn bộ user trong batch vừa thay đổi -> xoá cache Redis tương ứng.
                foreach (var userId in userRoles.Select(ur => ur.UserId).Distinct())
                {
                    await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(userId));
                }

                return _mapper.Map<List<UserRoleOutputDTO>>(userRoles);
            }, cancellationToken);
        }

        public async Task<string> ExportUserRoles(CancellationToken cancellationToken)
        {
            try
            {
                var userRoles = await _dbContext.QueryAsync<UserRoleEntity>(
                    @"Select * From tbl_User_Role Order By AutoId Asc",
                    cancellationToken: cancellationToken);

                var records = userRoles.Select(ur => _mapper.Map<UserRoleOutputDTO>(ur)).ToList();

                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<UserRoleExportMap>();
                    await csvWriter.WriteRecordsAsync(records, cancellationToken);
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<RoleOutputDTO>> GetRolesByUser(string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new AppException($"[{_errorPrefix}GetRolesByUser-01] UserId is required.", HttpStatusCode.BadRequest);
                }

                var roles = await _dbContext.QueryAsync<RoleEntity>(@"
                    Select R.* From tbl_Roles R
                    Inner Join tbl_User_Role UR On UR.RoleId = R.RoleId
                    Where UR.UserId = @UserId And UR.Status = 1 And R.Status = 1
                    Order By R.AutoId Asc",
                    new { UserId = userId },
                    cancellationToken);

                return roles.Select(r => _mapper.Map<RoleOutputDTO>(r)).ToList();
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Helpers
        private async Task<bool> CheckUserRoleExists(string userId, string roleId, CancellationToken cancellationToken)
        {
            try
            {
                var userRole = await _dbContext.QueryFirstOrDefaultAsync<UserRoleEntity>(
                    @"Select * From tbl_User_Role
                    Where UserId = @UserId And RoleId = @RoleId",
                    new { UserId = userId, RoleId = roleId },
                    cancellationToken);

                return userRole != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private sealed class UserRoleExportMap : ClassMap<UserRoleOutputDTO>
        {
            public UserRoleExportMap()
            {
                Map(m => m.UserId);
                Map(m => m.RoleId);
                Map(m => m.Status);
                Map(m => m.CreatedDate);
                Map(m => m.CreatedBy);
                Map(m => m.UpdatedDate);
                Map(m => m.UpdatedBy);
            }
        }
        #endregion
    }
}
