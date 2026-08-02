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

namespace Site.Infrastructure.Repositories
{
    public class FunctionRepository : BaseRepository<FunctionRepository>, IFunctionRepository
    {
        private readonly SiteDbContext _dbContext;
        private readonly IRedisService _redisService;
        private readonly string _errorPrefix = "Function_Repo_";

        public FunctionRepository(IMapper mapper, ILogger<FunctionRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService, IRedisService redisService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
            _redisService = redisService;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<FunctionOutputDTO> CreateFunction(FunctionInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateFunction-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.FunctionId))
                {
                    throw new AppException($"[{_errorPrefix}CreateFunction-02] FunctionId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.FunctionName))
                {
                    throw new AppException($"[{_errorPrefix}CreateFunction-03] FunctionName is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.ScreenId))
                {
                    throw new AppException($"[{_errorPrefix}CreateFunction-04] ScreenId is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckFunctionExists(input.FunctionId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateFunction-05] Function with FunctionId '{input.FunctionId}' already exists.", HttpStatusCode.Conflict);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_Functions (FunctionId, FunctionName, ScreenId, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@FunctionId, @FunctionName, @ScreenId, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    FunctionId = input.FunctionId,
                    FunctionName = input.FunctionName,
                    ScreenId = input.ScreenId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateFunction-06] Failed to create function.", HttpStatusCode.InternalServerError);
                }
                return _mapper.Map<FunctionOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<FunctionOutputDTO> UpdateFunction(FunctionInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}UpdateFunction-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.FunctionId))
                {
                    throw new AppException($"[{_errorPrefix}UpdateFunction-02] FunctionId is required.", HttpStatusCode.BadRequest);
                }
                if (!await CheckFunctionExists(input.FunctionId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}UpdateFunction-03] Function with FunctionId '{input.FunctionId}' not found.", HttpStatusCode.NotFound);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Functions
                        Set FunctionName = @FunctionName,
                            ScreenId = @ScreenId,
                            Status = @Status,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where FunctionId = @FunctionId
                    ", new
                {
                    FunctionId = input.FunctionId,
                    FunctionName = input.FunctionName,
                    ScreenId = input.ScreenId,
                    Status = input.Status.HasValue ? (int)input.Status.Value : 1,
                    UpdatedDate = DateTime.UtcNow,
                    UpdatedBy = input.UpdatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                }, cancellationToken);

                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}UpdateFunction-04] Failed to update function.", HttpStatusCode.InternalServerError);
                }

                // FunctionName/ScreenId/Status vừa đổi -> xoá cache Redis của mọi user có Role đang gán Function này.
                await InvalidateUsersCacheByFunction(input.FunctionId, cancellationToken);

                return _mapper.Map<FunctionOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<FunctionOutputDTO> GetFunctionById(string functionId, CancellationToken cancellationToken)
        {
            try
            {
                var function = await _dbContext.QueryFirstOrDefaultAsync<FunctionEntity>(
                    @"Select * From tbl_Functions
                    Where FunctionId = @FunctionId",
                    new { FunctionId = functionId },
                    cancellationToken);
                if (function == null)
                {
                    throw new AppException($"[{_errorPrefix}GetFunctionById-01] Function with FunctionId '{functionId}' not found.", HttpStatusCode.NotFound);
                }
                return _mapper.Map<FunctionOutputDTO>(function);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<FunctionOutputDTO>> GetAllFunctions(FunctionFilterDTO filter, CancellationToken cancellationToken)
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
                if (!string.IsNullOrWhiteSpace(filter.FunctionName))
                {
                    whereBuilder.Append(" AND FunctionName like @FunctionName ");
                    parameters.Add("FunctionName", $"%{filter.FunctionName.Trim()}%", System.Data.DbType.String);
                }
                if (!string.IsNullOrWhiteSpace(filter.ScreenId))
                {
                    whereBuilder.Append(" AND ScreenId = @ScreenId ");
                    parameters.Add("ScreenId", filter.ScreenId.Trim());
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
                string countSql = $"SELECT COUNT(1) FROM tbl_Functions {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<FunctionOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<FunctionOutputDTO>()
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

                string querySql = $"SELECT * FROM tbl_Functions {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var functions = await _dbContext.QueryAsync<FunctionEntity>(querySql, parameters, cancellationToken);

                filterOutput.Records = functions.Select(f => _mapper.Map<FunctionOutputDTO>(f)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteFunction(DeleteFunctionInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.FunctionId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteFunction-01] FunctionId is required.", HttpStatusCode.BadRequest);
                }
                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Functions
                        Set Status = -1,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where FunctionId = @FunctionId
                        ", new
                    {
                        FunctionId = input.FunctionId,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId
                    },
                        cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteFunction-02] Function with FunctionId '{input.FunctionId}' not found.", HttpStatusCode.NotFound);
                    }
                    await InvalidateUsersCacheByFunction(input.FunctionId, cancellationToken);
                    return input.FunctionId;
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_Functions
                        Where FunctionId = @FunctionId
                        ", new
                    {
                        FunctionId = input.FunctionId
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteFunction-02] Function with FunctionId '{input.FunctionId}' not found.", HttpStatusCode.NotFound);
                    }
                    await InvalidateUsersCacheByFunction(input.FunctionId, cancellationToken);
                    return input.FunctionId;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<FunctionOutputDTO>> ImportFunctions(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var functions = await FileHelper.ReadFile<FunctionInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<FunctionInputDTO>());

            if (!functions.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempFunctions
                    (
                        FunctionId NVARCHAR(20),
                        FunctionName NVARCHAR(255),
                        ScreenId NVARCHAR(50),
                        Status INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    functions,
                    "#TempFunctions",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_Functions AS T
                    USING #TempFunctions AS S
                    ON T.FunctionId = S.FunctionId

                    WHEN MATCHED THEN
                        UPDATE SET
                            T.FunctionName = S.FunctionName,
                            T.ScreenId = S.ScreenId,
                            T.Status = S.Status,
                            T.UpdatedDate = GETUTCDATE(),
                            T.UpdatedBy = @UpdatedBy,
                            T.DeviceName = @DeviceName,
                            T.DeviceId = @DeviceId

                    WHEN NOT MATCHED THEN
                        INSERT
                        (
                            FunctionId,
                            FunctionName,
                            ScreenId,
                            Status,
                            CreatedDate,
                            CreatedBy,
                            DeviceName,
                            DeviceId
                        )
                        VALUES
                        (
                            S.FunctionId,
                            S.FunctionName,
                            S.ScreenId,
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

                // Status/ScreenId của mọi Function trong batch có thể vừa đổi -> xoá cache Redis của user liên quan.
                foreach (var functionId in functions.Select(f => f.FunctionId).Distinct())
                {
                    await InvalidateUsersCacheByFunction(functionId, cancellationToken);
                }

                return _mapper.Map<List<FunctionOutputDTO>>(functions);
            }, cancellationToken);
        }

        public async Task<string> ExportFunctions(CancellationToken cancellationToken)
        {
            try
            {
                var functions = await _dbContext.QueryAsync<FunctionEntity>(
                    @"Select * From tbl_Functions Order By AutoId Asc",
                    cancellationToken: cancellationToken);

                var records = functions.Select(f => _mapper.Map<FunctionOutputDTO>(f)).ToList();

                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<FunctionExportMap>();
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
        private async Task<bool> CheckFunctionExists(string functionId, CancellationToken cancellationToken)
        {
            try
            {
                var function = await _dbContext.QueryFirstOrDefaultAsync<FunctionEntity>(
                    @"Select * From tbl_Functions
                    Where FunctionId = @FunctionId",
                    new { FunctionId = functionId },
                    cancellationToken);

                return function != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Xoá cache Redis (auth:user-permissions:{userId}) của mọi user có Role đang được gán Function này (qua
        /// Role_Function -> User_Role), để lần request kế tiếp của họ nạp lại đúng dữ liệu thay vì chờ hết TTL.
        /// </summary>
        private async Task InvalidateUsersCacheByFunction(string functionId, CancellationToken cancellationToken)
        {
            var userIds = await _dbContext.QueryAsync<string>(@"
                Select Distinct UR.UserId
                From tbl_Role_Function RF
                Inner Join tbl_User_Role UR On UR.RoleId = RF.RoleId
                Where RF.FunctionId = @FunctionId And RF.Status = 1 And UR.Status = 1",
                new { FunctionId = functionId },
                cancellationToken);

            foreach (var userId in userIds)
            {
                await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(userId));
            }
        }

        private sealed class FunctionExportMap : ClassMap<FunctionOutputDTO>
        {
            public FunctionExportMap()
            {
                Map(m => m.FunctionId);
                Map(m => m.FunctionName);
                Map(m => m.ScreenId);
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
