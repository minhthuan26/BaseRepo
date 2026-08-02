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
using FunctionEntity = Site.Domain.Entities.Function;
using RoleFunctionEntity = Site.Domain.Entities.RoleFunction;

namespace Site.Infrastructure.Repositories
{
    public class RoleFunctionRepository : BaseRepository<RoleFunctionRepository>, IRoleFunctionRepository
    {
        private readonly SiteDbContext _dbContext;
        private readonly IRedisService _redisService;
        private readonly string _errorPrefix = "RoleFunction_Repo_";

        public RoleFunctionRepository(IMapper mapper, ILogger<RoleFunctionRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService, IRedisService redisService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
            _redisService = redisService;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<RoleFunctionOutputDTO> CreateRoleFunction(RoleFunctionInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateRoleFunction-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.RoleId))
                {
                    throw new AppException($"[{_errorPrefix}CreateRoleFunction-02] RoleId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.FunctionId))
                {
                    throw new AppException($"[{_errorPrefix}CreateRoleFunction-03] FunctionId is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckRoleFunctionExists(input.RoleId, input.FunctionId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateRoleFunction-04] RoleFunction with RoleId '{input.RoleId}' and FunctionId '{input.FunctionId}' already exists.", HttpStatusCode.Conflict);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_Role_Function (RoleId, FunctionId, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@RoleId, @FunctionId, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    RoleId = input.RoleId,
                    FunctionId = input.FunctionId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateRoleFunction-05] Failed to create role function.", HttpStatusCode.InternalServerError);
                }

                // Function của Role vừa thay đổi -> xoá cache Redis của mọi user đang có Role này.
                await InvalidateUsersCacheByRole(input.RoleId, cancellationToken);

                return _mapper.Map<RoleFunctionOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<RoleFunctionOutputDTO> GetRoleFunctionById(string roleId, string functionId, CancellationToken cancellationToken)
        {
            try
            {
                var roleFunction = await _dbContext.QueryFirstOrDefaultAsync<RoleFunctionEntity>(
                    @"Select * From tbl_Role_Function
                    Where RoleId = @RoleId And FunctionId = @FunctionId",
                    new { RoleId = roleId, FunctionId = functionId },
                    cancellationToken);
                if (roleFunction == null)
                {
                    throw new AppException($"[{_errorPrefix}GetRoleFunctionById-01] RoleFunction with RoleId '{roleId}' and FunctionId '{functionId}' not found.", HttpStatusCode.NotFound);
                }
                return _mapper.Map<RoleFunctionOutputDTO>(roleFunction);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<RoleFunctionOutputDTO>> GetAllRoleFunctions(RoleFunctionFilterDTO filter, CancellationToken cancellationToken)
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
                if (!string.IsNullOrWhiteSpace(filter.RoleId))
                {
                    whereBuilder.Append(" AND RoleId = @RoleId ");
                    parameters.Add("RoleId", filter.RoleId.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filter.FunctionId))
                {
                    whereBuilder.Append(" AND FunctionId = @FunctionId ");
                    parameters.Add("FunctionId", filter.FunctionId.Trim());
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
                string countSql = $"SELECT COUNT(1) FROM tbl_Role_Function {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<RoleFunctionOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<RoleFunctionOutputDTO>()
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

                string querySql = $"SELECT * FROM tbl_Role_Function {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var roleFunctions = await _dbContext.QueryAsync<RoleFunctionEntity>(querySql, parameters, cancellationToken);

                filterOutput.Records = roleFunctions.Select(rf => _mapper.Map<RoleFunctionOutputDTO>(rf)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteRoleFunction(DeleteRoleFunctionInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.RoleId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteRoleFunction-01] RoleId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.FunctionId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteRoleFunction-02] FunctionId is required.", HttpStatusCode.BadRequest);
                }
                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Role_Function
                        Set Status = -1,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where RoleId = @RoleId And FunctionId = @FunctionId
                        ", new
                    {
                        RoleId = input.RoleId,
                        FunctionId = input.FunctionId,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId
                    },
                        cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteRoleFunction-03] RoleFunction with RoleId '{input.RoleId}' and FunctionId '{input.FunctionId}' not found.", HttpStatusCode.NotFound);
                    }
                    await InvalidateUsersCacheByRole(input.RoleId, cancellationToken);
                    return $"{input.RoleId}/{input.FunctionId}";
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_Role_Function
                        Where RoleId = @RoleId And FunctionId = @FunctionId
                        ", new
                    {
                        RoleId = input.RoleId,
                        FunctionId = input.FunctionId
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteRoleFunction-03] RoleFunction with RoleId '{input.RoleId}' and FunctionId '{input.FunctionId}' not found.", HttpStatusCode.NotFound);
                    }
                    await InvalidateUsersCacheByRole(input.RoleId, cancellationToken);
                    return $"{input.RoleId}/{input.FunctionId}";
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<RoleFunctionOutputDTO>> ImportRoleFunctions(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var roleFunctions = await FileHelper.ReadFile<RoleFunctionInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<RoleFunctionInputDTO>());

            if (!roleFunctions.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempRoleFunctions
                    (
                        RoleId NVARCHAR(50),
                        FunctionId NVARCHAR(20),
                        Status INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    roleFunctions,
                    "#TempRoleFunctions",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_Role_Function AS T
                    USING #TempRoleFunctions AS S
                    ON T.RoleId = S.RoleId AND T.FunctionId = S.FunctionId

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
                            RoleId,
                            FunctionId,
                            Status,
                            CreatedDate,
                            CreatedBy,
                            DeviceName,
                            DeviceId
                        )
                        VALUES
                        (
                            S.RoleId,
                            S.FunctionId,
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

                // Function của mọi Role xuất hiện trong batch vừa thay đổi -> xoá cache Redis của các user liên quan.
                foreach (var roleId in roleFunctions.Select(rf => rf.RoleId).Distinct())
                {
                    await InvalidateUsersCacheByRole(roleId, cancellationToken);
                }

                return _mapper.Map<List<RoleFunctionOutputDTO>>(roleFunctions);
            }, cancellationToken);
        }

        public async Task<string> ExportRoleFunctions(CancellationToken cancellationToken)
        {
            try
            {
                var roleFunctions = await _dbContext.QueryAsync<RoleFunctionEntity>(
                    @"Select * From tbl_Role_Function Order By AutoId Asc",
                    cancellationToken: cancellationToken);

                var records = roleFunctions.Select(rf => _mapper.Map<RoleFunctionOutputDTO>(rf)).ToList();

                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<RoleFunctionExportMap>();
                    await csvWriter.WriteRecordsAsync(records, cancellationToken);
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<FunctionOutputDTO>> GetFunctionsByRole(string roleId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId))
                {
                    throw new AppException($"[{_errorPrefix}GetFunctionsByRole-01] RoleId is required.", HttpStatusCode.BadRequest);
                }

                var functions = await _dbContext.QueryAsync<FunctionEntity>(@"
                    Select F.* From tbl_Functions F
                    Inner Join tbl_Role_Function RF On RF.FunctionId = F.FunctionId
                    Where RF.RoleId = @RoleId And RF.Status = 1 And F.Status = 1
                    Order By F.AutoId Asc",
                    new { RoleId = roleId },
                    cancellationToken);

                return functions.Select(f => _mapper.Map<FunctionOutputDTO>(f)).ToList();
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Helpers
        private async Task<bool> CheckRoleFunctionExists(string roleId, string functionId, CancellationToken cancellationToken)
        {
            try
            {
                var roleFunction = await _dbContext.QueryFirstOrDefaultAsync<RoleFunctionEntity>(
                    @"Select * From tbl_Role_Function
                    Where RoleId = @RoleId And FunctionId = @FunctionId",
                    new { RoleId = roleId, FunctionId = functionId },
                    cancellationToken);

                return roleFunction != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Xoá cache Redis (auth:user-permissions:{userId}) của mọi user đang có Role này, để lần request kế
        /// tiếp của họ nạp lại đúng danh sách Function mới thay vì chờ hết TTL (tối đa 1h).
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

        private sealed class RoleFunctionExportMap : ClassMap<RoleFunctionOutputDTO>
        {
            public RoleFunctionExportMap()
            {
                Map(m => m.RoleId);
                Map(m => m.FunctionId);
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
