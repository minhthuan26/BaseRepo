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

namespace Site.Infrastructure.Repositories
{
    public class RoleRepository : BaseRepository<RoleRepository>, IRoleRepository
    {
        private readonly SiteDbContext _dbContext;
        private readonly IRedisService _redisService;
        private readonly string _errorPrefix = "Role_Repo_";

        public RoleRepository(IMapper mapper, ILogger<RoleRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService, IRedisService redisService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
            _redisService = redisService;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<RoleOutputDTO> CreateRole(RoleInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateRole-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.RoleId))
                {
                    throw new AppException($"[{_errorPrefix}CreateRole-02] RoleId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.RoleName))
                {
                    throw new AppException($"[{_errorPrefix}CreateRole-03] RoleName is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}CreateRole-04] SiteId is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckRoleExists(input.RoleId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateRole-05] Role with RoleId '{input.RoleId}' already exists.", HttpStatusCode.Conflict);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_Roles (RoleId, RoleName, SiteId, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@RoleId, @RoleName, @SiteId, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    RoleId = input.RoleId,
                    RoleName = input.RoleName,
                    SiteId = input.SiteId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateRole-06] Failed to create role.", HttpStatusCode.InternalServerError);
                }
                return _mapper.Map<RoleOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<RoleOutputDTO> UpdateRole(RoleInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}UpdateRole-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.RoleId))
                {
                    throw new AppException($"[{_errorPrefix}UpdateRole-02] RoleId is required.", HttpStatusCode.BadRequest);
                }
                if (!await CheckRoleExists(input.RoleId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}UpdateRole-03] Role with RoleId '{input.RoleId}' not found.", HttpStatusCode.NotFound);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Roles
                        Set RoleName = @RoleName,
                            SiteId = @SiteId,
                            Status = @Status,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where RoleId = @RoleId
                    ", new
                {
                    RoleId = input.RoleId,
                    RoleName = input.RoleName,
                    SiteId = input.SiteId,
                    Status = input.Status.HasValue ? (int)input.Status.Value : 1,
                    UpdatedDate = DateTime.UtcNow,
                    UpdatedBy = input.UpdatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                }, cancellationToken);

                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}UpdateRole-04] Failed to update role.", HttpStatusCode.InternalServerError);
                }

                // RoleName/Status vừa đổi -> xoá cache Redis của mọi user đang có Role này.
                await InvalidateUsersCacheByRole(input.RoleId, cancellationToken);

                return _mapper.Map<RoleOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<RoleOutputDTO> GetRoleById(string roleId, CancellationToken cancellationToken)
        {
            try
            {
                var role = await _dbContext.QueryFirstOrDefaultAsync<RoleEntity>(
                    @"Select * From tbl_Roles
                    Where RoleId = @RoleId",
                    new { RoleId = roleId },
                    cancellationToken);
                if (role == null)
                {
                    throw new AppException($"[{_errorPrefix}GetRoleById-01] Role with RoleId '{roleId}' not found.", HttpStatusCode.NotFound);
                }
                return _mapper.Map<RoleOutputDTO>(role);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<RoleOutputDTO>> GetAllRoles(RoleFilterDTO filter, CancellationToken cancellationToken)
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
                if (!string.IsNullOrWhiteSpace(filter.RoleName))
                {
                    whereBuilder.Append(" AND RoleName like @RoleName ");
                    parameters.Add("RoleName", $"%{filter.RoleName.Trim()}%", System.Data.DbType.String);
                }
                if (!string.IsNullOrWhiteSpace(filter.SiteId))
                {
                    whereBuilder.Append(" AND SiteId = @SiteId ");
                    parameters.Add("SiteId", filter.SiteId.Trim());
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
                string countSql = $"SELECT COUNT(1) FROM tbl_Roles {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<RoleOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<RoleOutputDTO>()
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

                string querySql = $"SELECT * FROM tbl_Roles {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var roles = await _dbContext.QueryAsync<RoleEntity>(querySql, parameters, cancellationToken);

                filterOutput.Records = roles.Select(r => _mapper.Map<RoleOutputDTO>(r)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteRole(DeleteRoleInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.RoleId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteRole-01] RoleId is required.", HttpStatusCode.BadRequest);
                }
                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Roles
                        Set Status = -1,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where RoleId = @RoleId
                        ", new
                    {
                        RoleId = input.RoleId,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId
                    },
                        cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteRole-02] Role with RoleId '{input.RoleId}' not found.", HttpStatusCode.NotFound);
                    }
                    await InvalidateUsersCacheByRole(input.RoleId, cancellationToken);
                    return input.RoleId;
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_Roles
                        Where RoleId = @RoleId
                        ", new
                    {
                        RoleId = input.RoleId
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteRole-02] Role with RoleId '{input.RoleId}' not found.", HttpStatusCode.NotFound);
                    }
                    await InvalidateUsersCacheByRole(input.RoleId, cancellationToken);
                    return input.RoleId;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<RoleOutputDTO>> ImportRoles(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var roles = await FileHelper.ReadFile<RoleInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<RoleInputDTO>());

            if (!roles.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempRoles
                    (
                        RoleId NVARCHAR(20),
                        RoleName NVARCHAR(255),
                        SiteId NVARCHAR(50),
                        Status INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    roles,
                    "#TempRoles",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_Roles AS T
                    USING #TempRoles AS S
                    ON T.RoleId = S.RoleId

                    WHEN MATCHED THEN
                        UPDATE SET
                            T.RoleName = S.RoleName,
                            T.SiteId = S.SiteId,
                            T.Status = S.Status,
                            T.UpdatedDate = GETUTCDATE(),
                            T.UpdatedBy = @UpdatedBy,
                            T.DeviceName = @DeviceName,
                            T.DeviceId = @DeviceId

                    WHEN NOT MATCHED THEN
                        INSERT
                        (
                            RoleId,
                            RoleName,
                            SiteId,
                            Status,
                            CreatedDate,
                            CreatedBy,
                            DeviceName,
                            DeviceId
                        )
                        VALUES
                        (
                            S.RoleId,
                            S.RoleName,
                            S.SiteId,
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

                // RoleName/Status của mọi Role trong batch có thể vừa đổi -> xoá cache Redis của user liên quan.
                foreach (var roleId in roles.Select(r => r.RoleId).Distinct())
                {
                    await InvalidateUsersCacheByRole(roleId, cancellationToken);
                }

                return _mapper.Map<List<RoleOutputDTO>>(roles);
            }, cancellationToken);
        }

        public async Task<string> ExportRoles(CancellationToken cancellationToken)
        {
            try
            {
                var roles = await _dbContext.QueryAsync<RoleEntity>(
                    @"Select * From tbl_Roles Order By AutoId Asc",
                    cancellationToken: cancellationToken);

                var records = roles.Select(r => _mapper.Map<RoleOutputDTO>(r)).ToList();

                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<RoleExportMap>();
                    await csvWriter.WriteRecordsAsync(records, cancellationToken);
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Helpers
        private async Task<bool> CheckRoleExists(string roleId, CancellationToken cancellationToken)
        {
            try
            {
                var role = await _dbContext.QueryFirstOrDefaultAsync<RoleEntity>(
                    @"Select * From tbl_Roles
                    Where RoleId = @RoleId",
                    new { RoleId = roleId },
                    cancellationToken);

                return role != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Xoá cache Redis (auth:user-permissions:{userId}) của mọi user đang có Role này, để lần request kế
        /// tiếp của họ nạp lại đúng RoleName/Status/Function mới thay vì chờ hết TTL (tối đa 1h).
        /// </summary>
        private async Task InvalidateUsersCacheByRole(string roleId, CancellationToken cancellationToken)
        {
            var userIds = await _dbContext.QueryAsync<string>(@"
                Select Distinct UserId From tbl_User_Role Where RoleId = @RoleId And Status = 1",
                new { RoleId = roleId },
                cancellationToken);

            foreach (var userId in userIds)
            {
                await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(userId));
            }
        }

        private sealed class RoleExportMap : ClassMap<RoleOutputDTO>
        {
            public RoleExportMap()
            {
                Map(m => m.RoleId);
                Map(m => m.RoleName);
                Map(m => m.SiteId);
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
