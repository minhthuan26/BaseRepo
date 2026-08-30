using AutoMapper;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Services;
using CommonLibs.Utils;
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FunctionEntity = Site.Domain.Entities.Function;
using ModuleEntity = Site.Domain.Entities.Module;
using ScreenEntity = Site.Domain.Entities.Screen;

namespace Site.Infrastructure.Repositories
{
    public class PermissionRepository : BaseRepository<PermissionRepository>, IPermissionRepository
    {
        private readonly SiteDbContext _dbContext;
        private readonly IRedisService _redisService;
        private readonly string _errorPrefix = "Permission_Repo_";

        public PermissionRepository(IMapper mapper, ILogger<PermissionRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService, IRedisService redisService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
            _redisService = redisService;
        }

        #region Repository Methods
        public async Task<PermissionTreeDTO> GetPermissionTree(string siteId, string? roleId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                {
                    throw new AppException($"[{_errorPrefix}GetPermissionTree-01] SiteId is required.", HttpStatusCode.BadRequest);
                }

                var tree = new PermissionTreeDTO { SiteId = siteId, RoleId = roleId };

                // tbl_Modules.SiteId đã bị xoá (migration 04_globalize_module_catalog.sql - Module giờ là danh
                // mục dùng chung toàn hệ thống). "Site nào có Module nào" giờ xác định qua entitlement
                // tbl_Site_Module (bảng bật/tắt theo Site, xem 03_create_site_module.sql), không còn cột trực
                // tiếp trên tbl_Modules nữa - phải Join qua bảng này để biết Module nào thực sự thuộc/được bật
                // cho đúng SiteId đang xem cây quyền.
                var modules = (await _dbContext.QueryAsync<ModuleEntity>(@"
                    Select m.* From tbl_Modules m
                    Inner Join tbl_Site_Module sm On sm.ModuleId = m.ModuleId
                    Where sm.SiteId = @SiteId And sm.Status = 1 And m.Status = 1
                    Order By m.AutoId Asc",
                    new { SiteId = siteId }, cancellationToken)).ToList();

                if (!modules.Any())
                {
                    return tree;
                }

                var moduleIds = modules.Select(m => m.ModuleId).ToList();
                var screens = (await _dbContext.QueryAsync<ScreenEntity>(
                    "Select * From tbl_Screens Where ModuleId In @ModuleIds And Status = 1 Order By AutoId Asc",
                    new { ModuleIds = moduleIds }, cancellationToken)).ToList();

                var screenIds = screens.Select(s => s.ScreenId).ToList();
                var functions = screenIds.Any()
                    ? (await _dbContext.QueryAsync<FunctionEntity>(
                        "Select * From tbl_Functions Where ScreenId In @ScreenIds And Status = 1 Order By AutoId Asc",
                        new { ScreenIds = screenIds }, cancellationToken)).ToList()
                    : new List<FunctionEntity>();

                var assignedFunctionIds = new HashSet<string>();
                if (!string.IsNullOrWhiteSpace(roleId))
                {
                    // Lọc thêm SiteId - RoleId không còn unique toàn cục kể từ 08_fix_roleid_unique_scope.sql.
                    // Thiếu điều kiện này có thể vô tình đọc/hiển thị IsAssigned theo Role trùng RoleId của Site
                    // khác (siteId truyền vào roleId không thuộc về).
                    var assigned = await _dbContext.QueryAsync<string>(
                        "Select FunctionId From tbl_Role_Function Where RoleId = @RoleId And SiteId = @SiteId And Status = 1",
                        new { RoleId = roleId, SiteId = siteId }, cancellationToken);
                    assignedFunctionIds = assigned.ToHashSet();
                }

                tree.Modules = modules.Select(module => new PermissionModuleDTO
                {
                    ModuleId = module.ModuleId,
                    ModuleName = module.ModuleName,
                    Screens = screens.Where(s => s.ModuleId == module.ModuleId).Select(screen => new PermissionScreenDTO
                    {
                        ScreenId = screen.ScreenId,
                        ScreenName = screen.ScreenName,
                        Functions = functions.Where(f => f.ScreenId == screen.ScreenId).Select(function => new PermissionFunctionDTO
                        {
                            FunctionId = function.FunctionId,
                            FunctionName = function.FunctionName,
                            IsAssigned = assignedFunctionIds.Contains(function.FunctionId)
                        }).ToList()
                    }).ToList()
                }).ToList();

                return tree;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<string>> AssignRolePermissions(string roleId, List<string> functionIds, string siteId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId))
                {
                    throw new AppException($"[{_errorPrefix}AssignRolePermissions-01] RoleId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(siteId))
                {
                    throw new AppException($"[{_errorPrefix}AssignRolePermissions-03] SiteId is required.", HttpStatusCode.BadRequest);
                }

                // Lọc thêm SiteId - RoleId không còn unique toàn cục kể từ 08_fix_roleid_unique_scope.sql. Đồng
                // thời chặn Site A (dù có quyền Function AssignRolePermissions) cấp/thu quyền cho Role thuộc Site
                // B nếu tình cờ biết RoleId đó.
                var roleExists = await _dbContext.QueryFirstOrDefaultAsync<int?>(
                    "Select Top 1 1 From tbl_Roles Where RoleId = @RoleId And SiteId = @SiteId And Status = 1",
                    new { RoleId = roleId, SiteId = siteId }, cancellationToken);
                if (roleExists == null)
                {
                    throw new AppException($"[{_errorPrefix}AssignRolePermissions-02] Role with RoleId '{roleId}' not found in your Site.", HttpStatusCode.NotFound);
                }

                var desiredFunctionIds = (functionIds ?? new List<string>())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim())
                    .Distinct()
                    .ToList();

                var existingRows = (await _dbContext.QueryAsync<RoleFunctionStatusRow>(
                    "Select FunctionId, Status From tbl_Role_Function Where RoleId = @RoleId And SiteId = @SiteId",
                    new { RoleId = roleId, SiteId = siteId }, cancellationToken)).ToList();

                var existingMap = existingRows.ToDictionary(r => r.FunctionId, r => r.Status);

                var toInsert = desiredFunctionIds.Where(id => !existingMap.ContainsKey(id)).ToList();
                var toReactivate = desiredFunctionIds.Where(id => existingMap.ContainsKey(id) && existingMap[id] != 1).ToList();
                var toDeactivate = existingRows.Where(r => r.Status == 1 && !desiredFunctionIds.Contains(r.FunctionId)).Select(r => r.FunctionId).ToList();

                await _dbContext.ExecuteWithTransactionAsync(async () =>
                {
                    foreach (var functionId in toInsert)
                    {
                        // SiteId bắt buộc phải set (NOT NULL từ 05_add_siteid_to_role_tables.sql) - câu INSERT cũ
                        // thiếu cột này sẽ crash.
                        await _dbContext.ExecuteAsync(@"
                            Insert Into tbl_Role_Function (RoleId, FunctionId, SiteId, Status, CreatedDate, CreatedBy, DeviceName, DeviceId)
                            Values (@RoleId, @FunctionId, @SiteId, 1, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)",
                            new
                            {
                                RoleId = roleId,
                                FunctionId = functionId,
                                SiteId = siteId,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = _auditContext.UserName,
                                DeviceName = _auditContext.DeviceName,
                                DeviceId = _auditContext.DeviceId,
                            }, cancellationToken);
                    }

                    if (toReactivate.Any())
                    {
                        await _dbContext.ExecuteAsync(@"
                            Update tbl_Role_Function
                            Set Status = 1, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy, DeviceName = @DeviceName, DeviceId = @DeviceId
                            Where RoleId = @RoleId And SiteId = @SiteId And FunctionId In @FunctionIds",
                            new
                            {
                                RoleId = roleId,
                                SiteId = siteId,
                                FunctionIds = toReactivate,
                                UpdatedDate = DateTime.UtcNow,
                                UpdatedBy = _auditContext.UserName,
                                DeviceName = _auditContext.DeviceName,
                                DeviceId = _auditContext.DeviceId,
                            }, cancellationToken);
                    }

                    if (toDeactivate.Any())
                    {
                        await _dbContext.ExecuteAsync(@"
                            Update tbl_Role_Function
                            Set Status = -1, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy, DeviceName = @DeviceName, DeviceId = @DeviceId
                            Where RoleId = @RoleId And SiteId = @SiteId And FunctionId In @FunctionIds",
                            new
                            {
                                RoleId = roleId,
                                SiteId = siteId,
                                FunctionIds = toDeactivate,
                                UpdatedDate = DateTime.UtcNow,
                                UpdatedBy = _auditContext.UserName,
                                DeviceName = _auditContext.DeviceName,
                                DeviceId = _auditContext.DeviceId,
                            }, cancellationToken);
                    }
                }, cancellationToken);

                // Quyền của mọi user đang có Role này vừa thay đổi -> xoá cache Redis tương ứng.
                await InvalidateUsersCacheByRole(roleId, siteId, cancellationToken);

                return desiredFunctionIds;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Xoá cache Redis (auth:user-permissions:{userId}) của mọi user đang có Role này, để lần request kế
        /// tiếp của họ nạp lại đúng danh sách Function mới thay vì chờ hết TTL (tối đa 1h).
        /// </summary>
        private async Task InvalidateUsersCacheByRole(string roleId, string siteId, CancellationToken cancellationToken)
        {
            // Khoá cache Redis dùng AutoId (Login()/AuthorizationMiddleware đã đổi sang AutoId từ UserId - UserId
            // không còn unique toàn cục) - Join thẳng sang tbl_Users theo (UserId, SiteId) của chính dòng
            // tbl_User_Role đó (không chỉ theo UserId) để lấy đúng AutoId, tránh xoá nhầm cache của 1 User trùng
            // UserId ở Site khác.
            //
            // Bắt buộc lọc thêm ur.SiteId = @SiteId - RoleId không còn unique toàn cục kể từ
            // 08_fix_roleid_unique_scope.sql, thiếu điều kiện này sẽ vô tình xoá luôn cache của User ở Site KHÁC
            // chỉ vì Role của Site đó tình cờ trùng RoleId với Role vừa đổi quyền ở Site hiện tại.
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

        private sealed class RoleFunctionStatusRow
        {
            public string FunctionId { get; set; } = string.Empty;
            public int Status { get; set; }
        }
        #endregion
    }
}
