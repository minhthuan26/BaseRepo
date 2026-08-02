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

                var modules = (await _dbContext.QueryAsync<ModuleEntity>(
                    "Select * From tbl_Modules Where SiteId = @SiteId And Status = 1 Order By AutoId Asc",
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
                    var assigned = await _dbContext.QueryAsync<string>(
                        "Select FunctionId From tbl_Role_Function Where RoleId = @RoleId And Status = 1",
                        new { RoleId = roleId }, cancellationToken);
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

        public async Task<List<string>> AssignRolePermissions(string roleId, List<string> functionIds, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId))
                {
                    throw new AppException($"[{_errorPrefix}AssignRolePermissions-01] RoleId is required.", HttpStatusCode.BadRequest);
                }

                var roleExists = await _dbContext.QueryFirstOrDefaultAsync<int?>(
                    "Select Top 1 1 From tbl_Roles Where RoleId = @RoleId And Status = 1",
                    new { RoleId = roleId }, cancellationToken);
                if (roleExists == null)
                {
                    throw new AppException($"[{_errorPrefix}AssignRolePermissions-02] Role with RoleId '{roleId}' not found.", HttpStatusCode.NotFound);
                }

                var desiredFunctionIds = (functionIds ?? new List<string>())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim())
                    .Distinct()
                    .ToList();

                var existingRows = (await _dbContext.QueryAsync<RoleFunctionStatusRow>(
                    "Select FunctionId, Status From tbl_Role_Function Where RoleId = @RoleId",
                    new { RoleId = roleId }, cancellationToken)).ToList();

                var existingMap = existingRows.ToDictionary(r => r.FunctionId, r => r.Status);

                var toInsert = desiredFunctionIds.Where(id => !existingMap.ContainsKey(id)).ToList();
                var toReactivate = desiredFunctionIds.Where(id => existingMap.ContainsKey(id) && existingMap[id] != 1).ToList();
                var toDeactivate = existingRows.Where(r => r.Status == 1 && !desiredFunctionIds.Contains(r.FunctionId)).Select(r => r.FunctionId).ToList();

                await _dbContext.ExecuteWithTransactionAsync(async () =>
                {
                    foreach (var functionId in toInsert)
                    {
                        await _dbContext.ExecuteAsync(@"
                            Insert Into tbl_Role_Function (RoleId, FunctionId, Status, CreatedDate, CreatedBy, DeviceName, DeviceId)
                            Values (@RoleId, @FunctionId, 1, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)",
                            new
                            {
                                RoleId = roleId,
                                FunctionId = functionId,
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
                            Where RoleId = @RoleId And FunctionId In @FunctionIds",
                            new
                            {
                                RoleId = roleId,
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
                            Where RoleId = @RoleId And FunctionId In @FunctionIds",
                            new
                            {
                                RoleId = roleId,
                                FunctionIds = toDeactivate,
                                UpdatedDate = DateTime.UtcNow,
                                UpdatedBy = _auditContext.UserName,
                                DeviceName = _auditContext.DeviceName,
                                DeviceId = _auditContext.DeviceId,
                            }, cancellationToken);
                    }
                }, cancellationToken);

                // Quyền của mọi user đang có Role này vừa thay đổi -> xoá cache Redis tương ứng.
                await InvalidateUsersCacheByRole(roleId, cancellationToken);

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

        private sealed class RoleFunctionStatusRow
        {
            public string FunctionId { get; set; } = string.Empty;
            public int Status { get; set; }
        }
        #endregion
    }
}
