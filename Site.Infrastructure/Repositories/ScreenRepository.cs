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
using ScreenEntity = Site.Domain.Entities.Screen;

namespace Site.Infrastructure.Repositories
{
    public class ScreenRepository : BaseRepository<ScreenRepository>, IScreenRepository
    {
        private readonly SiteDbContext _dbContext;
        private readonly string _errorPrefix = "Screen_Repo_";

        public ScreenRepository(IMapper mapper, ILogger<ScreenRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<ScreenOutputDTO> CreateScreen(ScreenInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateScreen-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.ScreenId))
                {
                    throw new AppException($"[{_errorPrefix}CreateScreen-02] ScreenId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.ScreenName))
                {
                    throw new AppException($"[{_errorPrefix}CreateScreen-03] ScreenName is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.ModuleId))
                {
                    throw new AppException($"[{_errorPrefix}CreateScreen-04] ModuleId is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckScreenExists(input.ScreenId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateScreen-05] Screen with ScreenId '{input.ScreenId}' already exists.", HttpStatusCode.Conflict);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_Screens (ScreenId, ScreenName, ModuleId, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@ScreenId, @ScreenName, @ModuleId, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    ScreenId = input.ScreenId,
                    ScreenName = input.ScreenName,
                    ModuleId = input.ModuleId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateScreen-06] Failed to create screen.", HttpStatusCode.InternalServerError);
                }
                return _mapper.Map<ScreenOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ScreenOutputDTO> UpdateScreen(ScreenInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}UpdateScreen-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.ScreenId))
                {
                    throw new AppException($"[{_errorPrefix}UpdateScreen-02] ScreenId is required.", HttpStatusCode.BadRequest);
                }
                if (!await CheckScreenExists(input.ScreenId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}UpdateScreen-03] Screen with ScreenId '{input.ScreenId}' not found.", HttpStatusCode.NotFound);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Screens
                        Set ScreenName = @ScreenName,
                            ModuleId = @ModuleId,
                            Status = @Status,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where ScreenId = @ScreenId
                    ", new
                {
                    ScreenId = input.ScreenId,
                    ScreenName = input.ScreenName,
                    ModuleId = input.ModuleId,
                    Status = input.Status.HasValue ? (int)input.Status.Value : 1,
                    UpdatedDate = DateTime.UtcNow,
                    UpdatedBy = input.UpdatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                }, cancellationToken);

                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}UpdateScreen-04] Failed to update screen.", HttpStatusCode.InternalServerError);
                }

                return _mapper.Map<ScreenOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ScreenOutputDTO> GetScreenById(string screenId, CancellationToken cancellationToken)
        {
            try
            {
                var screen = await _dbContext.QueryFirstOrDefaultAsync<ScreenEntity>(
                    @"Select * From tbl_Screens
                    Where ScreenId = @ScreenId",
                    new { ScreenId = screenId },
                    cancellationToken);
                if (screen == null)
                {
                    throw new AppException($"[{_errorPrefix}GetScreenById-01] Screen with ScreenId '{screenId}' not found.", HttpStatusCode.NotFound);
                }
                return _mapper.Map<ScreenOutputDTO>(screen);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<ScreenOutputDTO>> GetAllScreens(ScreenFilterDTO filter, CancellationToken cancellationToken)
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
                if (!string.IsNullOrWhiteSpace(filter.ScreenName))
                {
                    whereBuilder.Append(" AND ScreenName like @ScreenName ");
                    parameters.Add("ScreenName", $"%{filter.ScreenName.Trim()}%", System.Data.DbType.String);
                }
                if (!string.IsNullOrWhiteSpace(filter.ModuleId))
                {
                    whereBuilder.Append(" AND ModuleId = @ModuleId ");
                    parameters.Add("ModuleId", filter.ModuleId.Trim());
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
                string countSql = $"SELECT COUNT(1) FROM tbl_Screens {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<ScreenOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<ScreenOutputDTO>()
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

                string querySql = $"SELECT * FROM tbl_Screens {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var screens = await _dbContext.QueryAsync<ScreenEntity>(querySql, parameters, cancellationToken);

                filterOutput.Records = screens.Select(s => _mapper.Map<ScreenOutputDTO>(s)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteScreen(DeleteScreenInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.ScreenId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteScreen-01] ScreenId is required.", HttpStatusCode.BadRequest);
                }
                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Screens
                        Set Status = -1,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where ScreenId = @ScreenId
                        ", new
                    {
                        ScreenId = input.ScreenId,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId
                    },
                        cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteScreen-02] Screen with ScreenId '{input.ScreenId}' not found.", HttpStatusCode.NotFound);
                    }
                    return input.ScreenId;
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_Screens
                        Where ScreenId = @ScreenId
                        ", new
                    {
                        ScreenId = input.ScreenId
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteScreen-02] Screen with ScreenId '{input.ScreenId}' not found.", HttpStatusCode.NotFound);
                    }
                    return input.ScreenId;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ScreenOutputDTO>> ImportScreens(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var screens = await FileHelper.ReadFile<ScreenInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<ScreenInputDTO>());

            if (!screens.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempScreens
                    (
                        ScreenId NVARCHAR(20),
                        ScreenName NVARCHAR(255),
                        ModuleId NVARCHAR(50),
                        Status INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    screens,
                    "#TempScreens",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_Screens AS T
                    USING #TempScreens AS S
                    ON T.ScreenId = S.ScreenId

                    WHEN MATCHED THEN
                        UPDATE SET
                            T.ScreenName = S.ScreenName,
                            T.ModuleId = S.ModuleId,
                            T.Status = S.Status,
                            T.UpdatedDate = GETUTCDATE(),
                            T.UpdatedBy = @UpdatedBy,
                            T.DeviceName = @DeviceName,
                            T.DeviceId = @DeviceId

                    WHEN NOT MATCHED THEN
                        INSERT
                        (
                            ScreenId,
                            ScreenName,
                            ModuleId,
                            Status,
                            CreatedDate,
                            CreatedBy,
                            DeviceName,
                            DeviceId
                        )
                        VALUES
                        (
                            S.ScreenId,
                            S.ScreenName,
                            S.ModuleId,
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

                return _mapper.Map<List<ScreenOutputDTO>>(screens);
            }, cancellationToken);
        }

        public async Task<string> ExportScreens(CancellationToken cancellationToken)
        {
            try
            {
                var screens = await _dbContext.QueryAsync<ScreenEntity>(
                    @"Select * From tbl_Screens Order By AutoId Asc",
                    cancellationToken: cancellationToken);

                var records = screens.Select(s => _mapper.Map<ScreenOutputDTO>(s)).ToList();

                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<ScreenExportMap>();
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
        private async Task<bool> CheckScreenExists(string screenId, CancellationToken cancellationToken)
        {
            try
            {
                var screen = await _dbContext.QueryFirstOrDefaultAsync<ScreenEntity>(
                    @"Select * From tbl_Screens
                    Where ScreenId = @ScreenId",
                    new { ScreenId = screenId },
                    cancellationToken);

                return screen != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private sealed class ScreenExportMap : ClassMap<ScreenOutputDTO>
        {
            public ScreenExportMap()
            {
                Map(m => m.ScreenId);
                Map(m => m.ScreenName);
                Map(m => m.ModuleId);
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
