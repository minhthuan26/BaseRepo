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
using ModuleEntity = Site.Domain.Entities.Module;

namespace Site.Infrastructure.Repositories
{
    public class ModuleRepository : BaseRepository<ModuleRepository>, IModuleRepository
    {
        private readonly SiteDbContext _dbContext;
        private readonly string _errorPrefix = "Module_Repo_";

        public ModuleRepository(IMapper mapper, ILogger<ModuleRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<ModuleOutputDTO> CreateModule(ModuleInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateModule-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.ModuleId))
                {
                    throw new AppException($"[{_errorPrefix}CreateModule-02] ModuleId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.ModuleName))
                {
                    throw new AppException($"[{_errorPrefix}CreateModule-03] ModuleName is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckModuleExists(input.ModuleId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateModule-05] Module with ModuleId '{input.ModuleId}' already exists.", HttpStatusCode.Conflict);
                }

                // Module là danh mục dùng chung toàn hệ thống (tbl_Modules.SiteId đã bị xoá - xem
                // CommonLibs/BaseSQL/multi-tenant/04_globalize_module_catalog.sql), không còn SiteId khi tạo mới.
                // Đây là API quản trị danh mục toàn cục (thêm loại Module MỚI cho cả hệ thống) - KHÔNG tự động
                // bật Module cho Site nào cả; muốn Site nào dùng được Module này phải provision riêng qua
                // tbl_Site_Module (xem SiteModuleController, đúng ý "provisioning là việc của đội vận hành" đã
                // ghi trong 03_create_site_module.sql).
                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_Modules (ModuleId, ModuleName, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@ModuleId, @ModuleName, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    ModuleId = input.ModuleId,
                    ModuleName = input.ModuleName,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateModule-06] Failed to create module.", HttpStatusCode.InternalServerError);
                }
                return _mapper.Map<ModuleOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ModuleOutputDTO> UpdateModule(ModuleInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}UpdateModule-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.ModuleId))
                {
                    throw new AppException($"[{_errorPrefix}UpdateModule-02] ModuleId is required.", HttpStatusCode.BadRequest);
                }
                if (!await CheckModuleExists(input.ModuleId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}UpdateModule-03] Module with ModuleId '{input.ModuleId}' not found.", HttpStatusCode.NotFound);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Modules
                        Set ModuleName = @ModuleName,
                            Status = @Status,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where ModuleId = @ModuleId
                    ", new
                {
                    ModuleId = input.ModuleId,
                    ModuleName = input.ModuleName,
                    Status = input.Status.HasValue ? (int)input.Status.Value : 1,
                    UpdatedDate = DateTime.UtcNow,
                    UpdatedBy = input.UpdatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                }, cancellationToken);

                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}UpdateModule-04] Failed to update module.", HttpStatusCode.InternalServerError);
                }

                return _mapper.Map<ModuleOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ModuleOutputDTO> GetModuleById(string moduleId, CancellationToken cancellationToken)
        {
            try
            {
                var module = await _dbContext.QueryFirstOrDefaultAsync<ModuleEntity>(
                    @"Select * From tbl_Modules
                    Where ModuleId = @ModuleId",
                    new { ModuleId = moduleId },
                    cancellationToken);
                if (module == null)
                {
                    throw new AppException($"[{_errorPrefix}GetModuleById-01] Module with ModuleId '{moduleId}' not found.", HttpStatusCode.NotFound);
                }
                return _mapper.Map<ModuleOutputDTO>(module);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<ModuleOutputDTO>> GetAllModules(ModuleFilterDTO filter, CancellationToken cancellationToken)
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
                if (!string.IsNullOrWhiteSpace(filter.ModuleName))
                {
                    whereBuilder.Append(" AND ModuleName like @ModuleName ");
                    parameters.Add("ModuleName", $"%{filter.ModuleName.Trim()}%", System.Data.DbType.String);
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
                string countSql = $"SELECT COUNT(1) FROM tbl_Modules {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<ModuleOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<ModuleOutputDTO>()
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

                string querySql = $"SELECT * FROM tbl_Modules {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var modules = await _dbContext.QueryAsync<ModuleEntity>(querySql, parameters, cancellationToken);

                filterOutput.Records = modules.Select(m => _mapper.Map<ModuleOutputDTO>(m)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteModule(DeleteModuleInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.ModuleId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteModule-01] ModuleId is required.", HttpStatusCode.BadRequest);
                }
                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Modules
                        Set Status = -1,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where ModuleId = @ModuleId
                        ", new
                    {
                        ModuleId = input.ModuleId,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId
                    },
                        cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteModule-02] Module with ModuleId '{input.ModuleId}' not found.", HttpStatusCode.NotFound);
                    }
                    return input.ModuleId;
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_Modules
                        Where ModuleId = @ModuleId
                        ", new
                    {
                        ModuleId = input.ModuleId
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteModule-02] Module with ModuleId '{input.ModuleId}' not found.", HttpStatusCode.NotFound);
                    }
                    return input.ModuleId;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ModuleOutputDTO>> ImportModules(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var modules = await FileHelper.ReadFile<ModuleInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<ModuleInputDTO>());

            if (!modules.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempModules
                    (
                        ModuleId NVARCHAR(50),
                        ModuleName NVARCHAR(MAX),
                        Status INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    modules,
                    "#TempModules",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_Modules AS T
                    USING #TempModules AS S
                    ON T.ModuleId = S.ModuleId

                    WHEN MATCHED THEN
                        UPDATE SET
                            T.ModuleName = S.ModuleName,
                            T.Status = S.Status,
                            T.UpdatedDate = GETUTCDATE(),
                            T.UpdatedBy = @UpdatedBy,
                            T.DeviceName = @DeviceName,
                            T.DeviceId = @DeviceId

                    WHEN NOT MATCHED THEN
                        INSERT
                        (
                            ModuleId,
                            ModuleName,
                            Status,
                            CreatedDate,
                            CreatedBy,
                            DeviceName,
                            DeviceId
                        )
                        VALUES
                        (
                            S.ModuleId,
                            S.ModuleName,
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

                return _mapper.Map<List<ModuleOutputDTO>>(modules);
            }, cancellationToken);
        }

        public async Task<string> ExportModules(CancellationToken cancellationToken)
        {
            try
            {
                var modules = await _dbContext.QueryAsync<ModuleEntity>(
                    @"Select * From tbl_Modules Order By AutoId Asc",
                    cancellationToken: cancellationToken);

                var records = modules.Select(m => _mapper.Map<ModuleOutputDTO>(m)).ToList();

                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<ModuleExportMap>();
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
        private async Task<bool> CheckModuleExists(string moduleId, CancellationToken cancellationToken)
        {
            try
            {
                var module = await _dbContext.QueryFirstOrDefaultAsync<ModuleEntity>(
                    @"Select * From tbl_Modules
                    Where ModuleId = @ModuleId",
                    new { ModuleId = moduleId },
                    cancellationToken);

                return module != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private sealed class ModuleExportMap : ClassMap<ModuleOutputDTO>
        {
            public ModuleExportMap()
            {
                Map(m => m.ModuleId);
                Map(m => m.ModuleName);
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
