using AutoMapper;
using CommonLibs.DTOs;
using CommonLibs.Entities;
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
using Site.Contract.DTOs;
using Site.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SiteModuleEntity = Site.Domain.Entities.SiteModule;

namespace Site.Infrastructure.Repositories
{
    public class SiteModuleRepository : BaseRepository<SiteModuleRepository>, ISiteModuleRepository
    {
        private readonly SiteDbContext _dbContext;
        private readonly string _errorPrefix = "SiteModule_Repo_";

        public SiteModuleRepository(IMapper mapper, ILogger<SiteModuleRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Repository Methods
        public async Task<SiteModuleOutputDTO> ProvisionSiteModule(ProvisionSiteModuleInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}ProvisionSiteModule-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}ProvisionSiteModule-02] SiteId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.ModuleId))
                {
                    throw new AppException($"[{_errorPrefix}ProvisionSiteModule-03] ModuleId is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckSiteModuleExists(input.SiteId, input.ModuleId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}ProvisionSiteModule-04] Site '{input.SiteId}' already has an entitlement row for Module '{input.ModuleId}'.", HttpStatusCode.Conflict);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

                // Mặc định TẮT (Inactive) khi mới cấp entitlement - đúng nguyên tắc "mặc định tắt, Site nào cần
                // thì bật" (xem tài liệu thiết kế mục 04). Chỉ ghi đè khi ops chủ động truyền Status khác trong input.
                var initialStatus = input.Status.HasValue ? (int)input.Status.Value : (int)EntityStatus.Inactive;

                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_Site_Module (SiteId, ModuleId, Status, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@SiteId, @ModuleId, @Status, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    SiteId = input.SiteId,
                    ModuleId = input.ModuleId,
                    Status = initialStatus,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}ProvisionSiteModule-05] Failed to provision module for site.", HttpStatusCode.InternalServerError);
                }

                return _mapper.Map<SiteModuleOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<SiteModuleOutputDTO>> GetSiteModulesBySite(string siteId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                {
                    throw new AppException($"[{_errorPrefix}GetSiteModulesBySite-01] SiteId is required.", HttpStatusCode.BadRequest);
                }

                var siteModules = await _dbContext.QueryAsync<SiteModuleEntity>(@"
                    Select * From tbl_Site_Module
                    Where SiteId = @SiteId
                    Order By AutoId Asc",
                    new { SiteId = siteId },
                    cancellationToken);

                return siteModules.Select(sm => _mapper.Map<SiteModuleOutputDTO>(sm)).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<SiteModuleOutputDTO> ToggleSiteModule(string siteId, ToggleSiteModuleInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                {
                    throw new AppException($"[{_errorPrefix}ToggleSiteModule-01] SiteId is required.", HttpStatusCode.BadRequest);
                }
                if (input == null || string.IsNullOrWhiteSpace(input.ModuleId))
                {
                    throw new AppException($"[{_errorPrefix}ToggleSiteModule-02] ModuleId is required.", HttpStatusCode.BadRequest);
                }

                var targetStatus = input.Enable ? (int)EntityStatus.Active : (int)EntityStatus.Inactive;

                // Chỉ UPDATE, không bao giờ INSERT ở đây - Site chỉ được bật/tắt Module ĐÃ được đội vận hành cấp
                // (đã có dòng qua ProvisionSiteModule). rowsAffected = 0 nghĩa là chưa được cấp -> 404, không phải
                // lỗi hệ thống, để FE biết hiển thị đúng "Site chưa mua Module này" thay vì "có lỗi xảy ra".
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Site_Module
                        Set Status = @Status,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where SiteId = @SiteId And ModuleId = @ModuleId
                    ", new
                {
                    SiteId = siteId,
                    ModuleId = input.ModuleId,
                    Status = targetStatus,
                    UpdatedDate = DateTime.UtcNow,
                    UpdatedBy = _auditContext.UserName,
                    DeviceName = _auditContext.DeviceName,
                    DeviceId = _auditContext.DeviceId
                }, cancellationToken);

                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}ToggleSiteModule-03] Module '{input.ModuleId}' has not been provisioned for this Site yet. Contact the operations team.", HttpStatusCode.NotFound);
                }

                var updated = await _dbContext.QueryFirstOrDefaultAsync<SiteModuleEntity>(@"
                    Select * From tbl_Site_Module
                    Where SiteId = @SiteId And ModuleId = @ModuleId",
                    new { SiteId = siteId, ModuleId = input.ModuleId },
                    cancellationToken);

                return _mapper.Map<SiteModuleOutputDTO>(updated);
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Helpers
        private async Task<bool> CheckSiteModuleExists(string siteId, string moduleId, CancellationToken cancellationToken)
        {
            try
            {
                var siteModule = await _dbContext.QueryFirstOrDefaultAsync<SiteModuleEntity>(
                    @"Select * From tbl_Site_Module
                    Where SiteId = @SiteId And ModuleId = @ModuleId",
                    new { SiteId = siteId, ModuleId = moduleId },
                    cancellationToken);

                return siteModule != null;
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
