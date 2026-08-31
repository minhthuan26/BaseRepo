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
using Site.Contract.DTOs;
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
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}CreateRoleFunction-05] SiteId is required.", HttpStatusCode.BadRequest);
                }

                // tbl_Role_Function.SiteId là NOT NULL (từ 05_add_siteid_to_role_tables.sql) nhưng câu INSERT cũ
                // không set cột này -> crash. Tra SiteId từ chính Role - đồng thời xác nhận Role tồn tại VÀ thuộc
                // đúng Site của người gọi (input.SiteId - Controller gán từ claim "site_id", không đọc từ client).
                //
                // QUAN TRỌNG: phải lọc SiteId = @SiteId (Site của người gọi) NGAY TRONG câu SELECT này, KHÔNG được
                // resolve roleSiteId chỉ bằng RoleId rồi so sánh sau. Từ 08_fix_roleid_unique_scope.sql, RoleId
                // không còn unique toàn cục - lọc thiếu SiteId khiến QueryFirstOrDefaultAsync trả về MỘT TRONG HAI
                // dòng một cách không xác định (không ORDER BY) khi 2 Site cùng có RoleId trùng tên, có thể vô
                // tình chặn nhầm chính Role hợp lệ của Site người gọi - lỗi thật đã bị phát hiện chính lúc viết
                // test cách ly 2 Site (xem CommonLibs/BaseSQL/multi-tenant/tests/).
                var roleSiteId = await _dbContext.QueryFirstOrDefaultAsync<string>(
                    "Select SiteId From tbl_Roles Where RoleId = @RoleId And SiteId = @SiteId And Status = 1",
                    new { RoleId = input.RoleId, SiteId = input.SiteId }, cancellationToken);
                if (roleSiteId == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateRoleFunction-06] Role with RoleId '{input.RoleId}' not found in your Site.", HttpStatusCode.NotFound);
                }

                // Check trùng phải lọc theo SiteId - RoleId không còn unique toàn cục kể từ
                // 08_fix_roleid_unique_scope.sql, 2 Role trùng RoleId ở 2 Site khác nhau có thể được gán cùng
                // FunctionId độc lập với nhau.
                if (await CheckRoleFunctionExists(input.RoleId, input.FunctionId, roleSiteId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateRoleFunction-04] RoleFunction with RoleId '{input.RoleId}' and FunctionId '{input.FunctionId}' already exists in Site '{roleSiteId}'.", HttpStatusCode.Conflict);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_Role_Function (RoleId, FunctionId, SiteId, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@RoleId, @FunctionId, @SiteId, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    RoleId = input.RoleId,
                    FunctionId = input.FunctionId,
                    SiteId = roleSiteId,
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
                await InvalidateUsersCacheByRole(input.RoleId, roleSiteId, cancellationToken);

                var output = _mapper.Map<RoleFunctionOutputDTO>(input);
                output.SiteId = roleSiteId;
                return output;
            }
            catch
            {
                throw;
            }
        }

        public async Task<RoleFunctionOutputDTO> GetRoleFunctionById(string roleId, string functionId, string siteId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                {
                    throw new AppException($"[{_errorPrefix}GetRoleFunctionById-02] SiteId is required.", HttpStatusCode.BadRequest);
                }
                var roleFunction = await _dbContext.QueryFirstOrDefaultAsync<RoleFunctionEntity>(
                    @"Select * From tbl_Role_Function
                    Where RoleId = @RoleId And FunctionId = @FunctionId And SiteId = @SiteId",
                    new { RoleId = roleId, FunctionId = functionId, SiteId = siteId },
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
                if (string.IsNullOrWhiteSpace(filter.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}GetAllRoleFunctions-01] SiteId is required.", HttpStatusCode.BadRequest);
                }

                var whereBuilder = new StringBuilder(" WHERE SiteId = @SiteId ");
                var parameters = new DynamicParameters();
                parameters.Add("SiteId", filter.SiteId.Trim());
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
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteRoleFunction-04] SiteId is required.", HttpStatusCode.BadRequest);
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
                        Where RoleId = @RoleId And FunctionId = @FunctionId And SiteId = @SiteId
                        ", new
                    {
                        RoleId = input.RoleId,
                        FunctionId = input.FunctionId,
                        SiteId = input.SiteId,
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
                    await InvalidateUsersCacheByRole(input.RoleId, input.SiteId, cancellationToken);
                    return $"{input.RoleId}/{input.FunctionId}";
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_Role_Function
                        Where RoleId = @RoleId And FunctionId = @FunctionId And SiteId = @SiteId
                        ", new
                    {
                        RoleId = input.RoleId,
                        FunctionId = input.FunctionId,
                        SiteId = input.SiteId
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteRoleFunction-03] RoleFunction with RoleId '{input.RoleId}' and FunctionId '{input.FunctionId}' not found.", HttpStatusCode.NotFound);
                    }
                    await InvalidateUsersCacheByRole(input.RoleId, input.SiteId, cancellationToken);
                    return $"{input.RoleId}/{input.FunctionId}";
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<RoleFunctionOutputDTO>> ImportRoleFunctions(Stream stream, string fileName, string siteId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(siteId))
            {
                throw new AppException($"[{_errorPrefix}ImportRoleFunctions-01] SiteId is required.", HttpStatusCode.BadRequest);
            }

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

                // Chỉ MERGE các dòng mà RoleId thực sự thuộc đúng SiteId của người import (Inner Join tbl_Roles
                // lọc SiteId = @SiteId) - dòng nào trỏ tới RoleId của Site khác (hoặc RoleId không tồn tại) bị
                // loại khỏi tập nguồn S, không MERGE. ON RoleId+FunctionId+SiteId (không chỉ RoleId+FunctionId) để
                // không GHI ĐÈ nhầm RoleFunction của Role trùng RoleId ở Site khác.
                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_Role_Function AS T
                    USING (
                        SELECT S.RoleId, S.FunctionId, S.Status, R.SiteId
                        FROM #TempRoleFunctions S
                        INNER JOIN tbl_Roles R ON R.RoleId = S.RoleId AND R.SiteId = @SiteId AND R.Status = 1
                    ) AS S
                    ON T.RoleId = S.RoleId AND T.FunctionId = S.FunctionId AND T.SiteId = S.SiteId

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
                            S.FunctionId,
                            S.SiteId,
                            S.Status,
                            GETUTCDATE(),
                            @CreatedBy,
                            @DeviceName,
                            @DeviceId
                        );
                    ", new
                {
                    SiteId = siteId,
                    UpdatedBy = _auditContext.UserName,
                    CreatedBy = _auditContext.UserName,
                    DeviceName = _auditContext.DeviceName,
                    DeviceId = _auditContext.DeviceId,
                },
                    cancellationToken: cancellationToken);

                // Function của mọi Role xuất hiện trong batch vừa thay đổi -> xoá cache Redis của các user liên quan.
                foreach (var roleId in roleFunctions.Select(rf => rf.RoleId).Distinct())
                {
                    await InvalidateUsersCacheByRole(roleId, siteId, cancellationToken);
                }

                return _mapper.Map<List<RoleFunctionOutputDTO>>(roleFunctions);
            }, cancellationToken);
        }

        public async Task<string> ExportRoleFunctions(string siteId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                {
                    throw new AppException($"[{_errorPrefix}ExportRoleFunctions-01] SiteId is required.", HttpStatusCode.BadRequest);
                }

                var roleFunctions = await _dbContext.QueryAsync<RoleFunctionEntity>(
                    @"Select * From tbl_Role_Function Where SiteId = @SiteId Order By AutoId Asc",
                    new { SiteId = siteId },
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

        public async Task<List<FunctionOutputDTO>> GetFunctionsByRole(string roleId, string siteId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId))
                {
                    throw new AppException($"[{_errorPrefix}GetFunctionsByRole-01] RoleId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(siteId))
                {
                    throw new AppException($"[{_errorPrefix}GetFunctionsByRole-02] SiteId is required.", HttpStatusCode.BadRequest);
                }

                // Lọc thêm RF.SiteId - RoleId không còn unique toàn cục, thiếu điều kiện này có thể trả nhầm
                // Function của Role trùng RoleId ở Site khác.
                var functions = await _dbContext.QueryAsync<FunctionEntity>(@"
                    Select F.* From tbl_Functions F
                    Inner Join tbl_Role_Function RF On RF.FunctionId = F.FunctionId
                    Where RF.RoleId = @RoleId And RF.SiteId = @SiteId And RF.Status = 1 And F.Status = 1
                    Order By F.AutoId Asc",
                    new { RoleId = roleId, SiteId = siteId },
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
        private async Task<bool> CheckRoleFunctionExists(string roleId, string functionId, string siteId, CancellationToken cancellationToken)
        {
            try
            {
                var roleFunction = await _dbContext.QueryFirstOrDefaultAsync<RoleFunctionEntity>(
                    @"Select * From tbl_Role_Function
                    Where RoleId = @RoleId And FunctionId = @FunctionId And SiteId = @SiteId",
                    new { RoleId = roleId, FunctionId = functionId, SiteId = siteId },
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
        private async Task InvalidateUsersCacheByRole(string roleId, string siteId, CancellationToken cancellationToken)
        {
            // Khoá cache Redis dùng AutoId (không phải UserId - UserId không còn unique toàn cục kể từ khi
            // Login()/AuthorizationMiddleware đổi sang AutoId). Join sang tbl_Users theo đúng (UserId, SiteId) của
            // từng dòng tbl_User_Role để lấy đúng AutoId, tránh xoá nhầm cache của User trùng UserId ở Site khác.
            //
            // Bắt buộc lọc thêm ur.SiteId = @SiteId - RoleId không còn unique toàn cục kể từ
            // 08_fix_roleid_unique_scope.sql, thiếu điều kiện này sẽ vô tình xoá luôn cache của User ở Site KHÁC
            // chỉ vì Role của Site đó tình cờ trùng RoleId với Role vừa cấp/thu Function ở Site hiện tại.
            var autoIds = await _dbContext.QueryAsync<long>(@"
                Select Distinct u.AutoId
                From tbl_User_Role ur
                Inner Join tbl_Users u On u.UserId = ur.UserId And u.SiteId = ur.SiteId And u.Status = 1
                Where ur.RoleId = @RoleId And ur.SiteId = @SiteId And ur.Status = 1",
                new { RoleId = roleId, SiteId = siteId },
                cancellationToken);

            foreach (var autoId in autoIds)
            {
                await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(autoId.ToString()));
            }
        }

        private sealed class RoleFunctionExportMap : ClassMap<RoleFunctionOutputDTO>
        {
            public RoleFunctionExportMap()
            {
                Map(m => m.RoleId);
                Map(m => m.FunctionId);
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
