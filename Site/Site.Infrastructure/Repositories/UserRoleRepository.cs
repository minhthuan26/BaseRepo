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
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-08] SiteId is required.", HttpStatusCode.BadRequest);
                }

                // Role phải thuộc 1 Site cụ thể. tbl_User_Site (quan hệ N-N User-Site) ĐÃ BỊ XOÁ ở migration
                // multi-tenant (xem CommonLibs/BaseSQL/multi-tenant/README.md) - User giờ chỉ thuộc đúng 1 Site,
                // gắn thẳng tbl_Users.SiteId. Điều kiện "user là thành viên Site" đổi thành so khớp trực tiếp
                // User.SiteId == Role.SiteId. Lọc theo (UserId, SiteId) ở đây vừa xác định đúng User (UserId
                // không còn unique toàn cục, chỉ unique theo từng Site) vừa enforce luôn ràng buộc "Role và User
                // phải cùng Site" trong 1 câu query.
                //
                // QUAN TRỌNG: câu SELECT phải lọc thêm SiteId = @SiteId (Site của NGƯỜI GỌI, input.SiteId - Controller
                // gán từ claim "site_id", không đọc từ client) ngay từ đầu, KHÔNG được resolve roleSiteId chỉ bằng
                // RoleId rồi so sánh sau. Từ 08_fix_roleid_unique_scope.sql, RoleId không còn unique toàn cục - nếu
                // chỉ lọc theo RoleId, 2 Site cùng có RoleId trùng tên (đúng kịch bản test cách ly 2 Site) sẽ khiến
                // QueryFirstOrDefaultAsync trả về MỘT TRONG HAI dòng một cách không xác định (không có ORDER BY) -
                // có thể vô tình chặn nhầm chính Role hợp lệ của Site người gọi (roleSiteId "trúng" ngẫu nhiên dòng
                // của Site kia), một lỗi thật đã bị phát hiện chính lúc viết test cách ly 2 Site.
                var roleSiteId = await _dbContext.QueryFirstOrDefaultAsync<string>(@"
                    Select SiteId From tbl_Roles Where RoleId = @RoleId And SiteId = @SiteId And Status = 1",
                    new { RoleId = input.RoleId, SiteId = input.SiteId },
                    cancellationToken);
                if (roleSiteId == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-05] Role with RoleId '{input.RoleId}' not found in your Site.", HttpStatusCode.NotFound);
                }

                // Check trùng PHẢI lọc theo SiteId của Role: (UserId, RoleId) không unique toàn cục, 2 Site khác
                // nhau hoàn toàn có thể có cùng cặp UserId/RoleId (đúng kịch bản test cách ly 2 Site) - nếu không
                // lọc SiteId, User ở Site B sẽ bị chặn nhầm "already exists" chỉ vì Site A tình cờ có cùng cặp ID.
                if (await CheckUserRoleExists(input.UserId, input.RoleId, roleSiteId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-04] UserRole with UserId '{input.UserId}' and RoleId '{input.RoleId}' already exists in Site '{roleSiteId}'.", HttpStatusCode.Conflict);
                }

                // Chỉ cần AutoId (khoá cache Redis) - không cần hydrate cả entity User, và Site service vốn
                // không có entity User riêng (tbl_Users thuộc Gateway) nên chỉ query scalar là đủ và an toàn.
                var userAutoId = await _dbContext.QueryFirstOrDefaultAsync<long?>(@"
                    Select AutoId From tbl_Users Where UserId = @UserId And SiteId = @SiteId And Status = 1",
                    new { UserId = input.UserId, SiteId = roleSiteId },
                    cancellationToken);
                if (userAutoId == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateUserRole-06] User '{input.UserId}' does not belong to Site '{roleSiteId}' (the Site this Role belongs to) - a User can only be assigned Roles from their own Site.", HttpStatusCode.BadRequest);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_User_Role (UserId, RoleId, SiteId, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@UserId, @RoleId, @SiteId, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    UserId = input.UserId,
                    RoleId = input.RoleId,
                    SiteId = roleSiteId,
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

                // Quyền của user vừa thay đổi (thêm 1 Role) -> xoá cache Redis để lần request kế tiếp nạp lại dữ
                // liệu mới. Khoá theo AutoId (không phải UserId - UserId không còn unique toàn cục) để khớp đúng
                // định dạng cache key mà Login()/AuthorizationMiddleware dùng (auth:user-permissions:{autoId}).
                await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(userAutoId.Value.ToString()));

                return _mapper.Map<UserRoleOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<UserRoleOutputDTO> GetUserRoleById(string userId, string roleId, string siteId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                {
                    throw new AppException($"[{_errorPrefix}GetUserRoleById-03] SiteId is required.", HttpStatusCode.BadRequest);
                }

                // tbl_User_Role.RoleId không unique toàn cục (mỗi Site có RoleId riêng, có thể trùng chuỗi giữa
                // các Site) và UserId cũng vậy. Where chỉ theo (UserId, RoleId) như code cũ có thể vô tình khớp
                // đúng cặp UserId/RoleId của MỘT SITE KHÁC nếu 2 Site tình cờ dùng cùng ID - đây chính là kịch bản
                // rò rỉ chéo Site mà test cách ly 2 Site cần chứng minh KHÔNG xảy ra.
                //
                // QUAN TRỌNG: siteId ở đây PHẢI là Site của người gọi (CurrentSiteId, Controller truyền xuống),
                // KHÔNG được resolve "Site của Role" chỉ bằng RoleId một mình - kể từ khi RoleId được phép trùng
                // giữa 2 Site (08_fix_roleid_unique_scope.sql), 1 câu Select SiteId From tbl_Roles Where RoleId =
                // @RoleId (không kèm SiteId) sẽ ambiguous, QueryFirstOrDefaultAsync trả về 1 trong 2 dòng một cách
                // không xác định (không ORDER BY) - lỗi thật đã bị phát hiện chính lúc viết test cách ly 2 Site.
                // Lọc thẳng theo (RoleId, SiteId = siteId) loại bỏ hoàn toàn sự mập mờ này, đồng thời tự nhiên trả
                // NotFound nếu Role đó không thuộc đúng Site của người gọi (không rò rỉ dữ liệu Site khác).
                var roleExists = await _dbContext.QueryFirstOrDefaultAsync<int?>(@"
                    Select Top 1 1 From tbl_Roles Where RoleId = @RoleId And SiteId = @SiteId And Status = 1",
                    new { RoleId = roleId, SiteId = siteId },
                    cancellationToken);
                if (roleExists == null)
                {
                    throw new AppException($"[{_errorPrefix}GetUserRoleById-01] UserRole with UserId '{userId}' and RoleId '{roleId}' not found.", HttpStatusCode.NotFound);
                }

                var userRole = await _dbContext.QueryFirstOrDefaultAsync<UserRoleEntity>(
                    @"Select * From tbl_User_Role
                    Where UserId = @UserId And RoleId = @RoleId And SiteId = @SiteId",
                    new { UserId = userId, RoleId = roleId, SiteId = siteId },
                    cancellationToken);
                if (userRole == null)
                {
                    throw new AppException($"[{_errorPrefix}GetUserRoleById-02] UserRole with UserId '{userId}' and RoleId '{roleId}' not found.", HttpStatusCode.NotFound);
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
                if (string.IsNullOrWhiteSpace(filter.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}GetAllUserRoles-01] SiteId is required.", HttpStatusCode.BadRequest);
                }

                var whereBuilder = new StringBuilder(" WHERE SiteId = @SiteId ");
                var parameters = new DynamicParameters();
                parameters.Add("SiteId", filter.SiteId.Trim());
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
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteUserRole-05] SiteId is required.", HttpStatusCode.BadRequest);
                }
                // RoleId chỉ thuộc đúng 1 Site - dùng để (a) lọc SiteId trên UPDATE/DELETE bên dưới (tránh xoá
                // nhầm bản ghi của Site khác có cùng cặp UserId/RoleId) và (b) tra AutoId đúng User để xoá cache
                // Redis đúng khoá (auth:user-permissions:{autoId}, không còn dùng UserId làm khoá).
                //
                // QUAN TRỌNG: phải lọc SiteId = @SiteId (Site của người gọi, input.SiteId - Controller gán từ
                // claim "site_id") NGAY TRONG câu SELECT, KHÔNG được resolve roleSiteId chỉ bằng RoleId một mình.
                // Từ 08_fix_roleid_unique_scope.sql, RoleId không còn unique toàn cục - lọc thiếu SiteId khiến
                // QueryFirstOrDefaultAsync trả về MỘT TRONG HAI dòng không xác định (không ORDER BY) khi 2 Site
                // cùng có RoleId trùng tên, có nguy cơ xoá nhầm UserRole của Site khác nếu tình cờ trùng cả
                // UserId - lỗi thật đã bị phát hiện chính lúc viết test cách ly 2 Site.
                var roleSiteId = await _dbContext.QueryFirstOrDefaultAsync<string>(@"
                    Select SiteId From tbl_Roles Where RoleId = @RoleId And SiteId = @SiteId And Status = 1",
                    new { RoleId = input.RoleId, SiteId = input.SiteId },
                    cancellationToken);
                if (roleSiteId == null)
                {
                    throw new AppException($"[{_errorPrefix}DeleteUserRole-03] UserRole with UserId '{input.UserId}' and RoleId '{input.RoleId}' not found in your Site.", HttpStatusCode.NotFound);
                }

                var userAutoId = await _dbContext.QueryFirstOrDefaultAsync<long?>(@"
                    Select AutoId From tbl_Users Where UserId = @UserId And SiteId = @SiteId And Status = 1",
                    new { UserId = input.UserId, SiteId = roleSiteId },
                    cancellationToken);

                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_User_Role
                        Set Status = -1,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where UserId = @UserId And RoleId = @RoleId And SiteId = @SiteId
                        ", new
                    {
                        UserId = input.UserId,
                        RoleId = input.RoleId,
                        SiteId = roleSiteId,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId
                    },
                        cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteUserRole-04] UserRole with UserId '{input.UserId}' and RoleId '{input.RoleId}' not found.", HttpStatusCode.NotFound);
                    }
                    if (userAutoId != null)
                    {
                        await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(userAutoId.Value.ToString()));
                    }
                    return $"{input.UserId}/{input.RoleId}";
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_User_Role
                        Where UserId = @UserId And RoleId = @RoleId And SiteId = @SiteId
                        ", new
                    {
                        UserId = input.UserId,
                        RoleId = input.RoleId,
                        SiteId = roleSiteId
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteUserRole-04] UserRole with UserId '{input.UserId}' and RoleId '{input.RoleId}' not found.", HttpStatusCode.NotFound);
                    }
                    if (userAutoId != null)
                    {
                        await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(userAutoId.Value.ToString()));
                    }
                    return $"{input.UserId}/{input.RoleId}";
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<UserRoleOutputDTO>> ImportUserRoles(Stream stream, string fileName, string siteId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(siteId))
            {
                throw new AppException($"[{_errorPrefix}ImportUserRoles-01] SiteId is required.", HttpStatusCode.BadRequest);
            }

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

                // Chỉ MERGE các dòng mà CẢ RoleId lẫn UserId đều thực sự thuộc đúng SiteId của người import (Inner
                // Join với tbl_Roles/tbl_Users lọc SiteId = @SiteId) - dòng nào trong file CSV trỏ tới RoleId/UserId
                // của Site khác (dù vô tình hay cố ý) sẽ bị loại thẳng khỏi tập nguồn S, không MERGE, không insert,
                // không update. Nhờ vậy vừa tránh crash NOT NULL (SiteId luôn được set từ chính tbl_Roles) vừa chặn
                // hẳn khả năng import chéo Site.
                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_User_Role AS T
                    USING (
                        SELECT S.UserId, S.RoleId, S.Status, R.SiteId
                        FROM #TempUserRoles S
                        INNER JOIN tbl_Roles R ON R.RoleId = S.RoleId AND R.SiteId = @SiteId AND R.Status = 1
                        INNER JOIN tbl_Users U ON U.UserId = S.UserId AND U.SiteId = @SiteId AND U.Status = 1
                    ) AS S
                    ON T.UserId = S.UserId AND T.RoleId = S.RoleId AND T.SiteId = S.SiteId

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
                            SiteId,
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

                // Quyền của toàn bộ user trong batch (thuộc đúng Site đang import) vừa thay đổi -> xoá cache Redis
                // theo AutoId (không phải UserId - UserId không còn unique toàn cục, khác định dạng khoá cache mà
                // Login()/AuthorizationMiddleware dùng).
                var affectedUserIds = userRoles.Select(ur => ur.UserId).Distinct().ToList();
                var affectedAutoIds = await _dbContext.QueryAsync<long>(@"
                    Select AutoId From tbl_Users Where SiteId = @SiteId And UserId In @UserIds And Status = 1",
                    new { SiteId = siteId, UserIds = affectedUserIds },
                    cancellationToken: cancellationToken);
                foreach (var autoId in affectedAutoIds)
                {
                    await _redisService.RemoveAsync(UserPermissionCache.GetCacheKey(autoId.ToString()));
                }

                return _mapper.Map<List<UserRoleOutputDTO>>(userRoles);
            }, cancellationToken);
        }

        public async Task<string> ExportUserRoles(string siteId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                {
                    throw new AppException($"[{_errorPrefix}ExportUserRoles-01] SiteId is required.", HttpStatusCode.BadRequest);
                }

                var userRoles = await _dbContext.QueryAsync<UserRoleEntity>(
                    @"Select * From tbl_User_Role Where SiteId = @SiteId Order By AutoId Asc",
                    new { SiteId = siteId },
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

        public async Task<List<RoleOutputDTO>> GetRolesByUser(string userId, string siteId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new AppException($"[{_errorPrefix}GetRolesByUser-01] UserId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(siteId))
                {
                    throw new AppException($"[{_errorPrefix}GetRolesByUser-02] SiteId is required.", HttpStatusCode.BadRequest);
                }

                // Bắt buộc lọc thêm UR.SiteId = @SiteId (và R.SiteId = @SiteId) - UserId/RoleId không còn unique
                // toàn cục, thiếu điều kiện Site sẽ gộp nhầm Role của 1 User trùng UserId ở Site khác.
                var roles = await _dbContext.QueryAsync<RoleEntity>(@"
                    Select R.* From tbl_Roles R
                    Inner Join tbl_User_Role UR On UR.RoleId = R.RoleId And UR.SiteId = R.SiteId
                    Where UR.UserId = @UserId And UR.SiteId = @SiteId And UR.Status = 1 And R.Status = 1
                    Order By R.AutoId Asc",
                    new { UserId = userId, SiteId = siteId },
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
        private async Task<bool> CheckUserRoleExists(string userId, string roleId, string siteId, CancellationToken cancellationToken)
        {
            try
            {
                var userRole = await _dbContext.QueryFirstOrDefaultAsync<UserRoleEntity>(
                    @"Select * From tbl_User_Role
                    Where UserId = @UserId And RoleId = @RoleId And SiteId = @SiteId",
                    new { UserId = userId, RoleId = roleId, SiteId = siteId },
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
