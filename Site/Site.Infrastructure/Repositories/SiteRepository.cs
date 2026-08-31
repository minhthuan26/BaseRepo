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
using SiteEntity = Site.Domain.Entities.Site;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Site.Infrastructure.Repositories
{
    public class SiteRepository : BaseRepository<SiteRepository>, ISiteRepository
    {
        private readonly SiteDbContext _dbContext;
        private readonly string _errorPrefix = "Site_Repo_";

        public SiteRepository(IMapper mapper, ILogger<SiteRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<SiteOutputDTO> CreateSite(SiteInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateSite-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}CreateSite-02] SiteId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.SiteName))
                {
                    throw new AppException($"[{_errorPrefix}CreateSite-03] SiteName is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckSiteExists(input.SiteId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateSite-04] Site with SiteId '{input.SiteId}' already exists.", HttpStatusCode.Conflict);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_Site (SiteId, SiteName, SiteLogo, Address, Email, Hotline, TaxCode, Representative, RepresentativeIdNumber, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@SiteId, @SiteName, @SiteLogo, @Address, @Email, @Hotline, @TaxCode, @Representative, @RepresentativeIdNumber, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    SiteId = input.SiteId,
                    SiteName = input.SiteName,
                    SiteLogo = input.SiteLogo,
                    Address = input.Address,
                    Email = input.Email,
                    Hotline = input.Hotline,
                    TaxCode = input.TaxCode,
                    Representative = input.Representative,
                    RepresentativeIdNumber = input.RepresentativeIdNumber,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateSite-05] Failed to create site.", HttpStatusCode.InternalServerError);
                }
                return _mapper.Map<SiteOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<SiteOutputDTO> UpdateSite(SiteInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}UpdateSite-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}UpdateSite-02] SiteId is required.", HttpStatusCode.BadRequest);
                }
                if (!await CheckSiteExists(input.SiteId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}UpdateSite-03] Site with SiteId '{input.SiteId}' not found.", HttpStatusCode.NotFound);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Site
                        Set SiteName = @SiteName,
                            SiteLogo = @SiteLogo,
                            Address = @Address,
                            Email = @Email,
                            Hotline = @Hotline,
                            TaxCode = @TaxCode,
                            Representative = @Representative,
                            RepresentativeIdNumber = @RepresentativeIdNumber,
                            Status = @Status,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where SiteId = @SiteId
                    ", new
                {
                    SiteId = input.SiteId,
                    SiteName = input.SiteName,
                    SiteLogo = input.SiteLogo,
                    Address = input.Address,
                    Email = input.Email,
                    Hotline = input.Hotline,
                    TaxCode = input.TaxCode,
                    Representative = input.Representative,
                    RepresentativeIdNumber = input.RepresentativeIdNumber,
                    Status = input.Status.HasValue ? (int)input.Status.Value : 1,
                    UpdatedDate = DateTime.UtcNow,
                    UpdatedBy = input.UpdatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                }, cancellationToken);

                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}UpdateSite-04] Failed to update site.", HttpStatusCode.InternalServerError);
                }

                return _mapper.Map<SiteOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<SiteOutputDTO> GetSiteById(string siteId, CancellationToken cancellationToken)
        {
            try
            {
                var site = await _dbContext.QueryFirstOrDefaultAsync<SiteEntity>(
                    @"Select * From tbl_Site
                    Where SiteId = @SiteId",
                    new { SiteId = siteId },
                    cancellationToken);
                if (site == null)
                {
                    throw new AppException($"[{_errorPrefix}GetSiteById-01] Site with SiteId '{siteId}' not found.", HttpStatusCode.NotFound);
                }
                return _mapper.Map<SiteOutputDTO>(site);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<SiteOutputDTO>> GetAllSites(SiteFilterDTO filter, CancellationToken cancellationToken)
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
                if (!string.IsNullOrWhiteSpace(filter.SiteName))
                {
                    whereBuilder.Append(" AND SiteName like @SiteName ");
                    parameters.Add("SiteName", $"%{filter.SiteName.Trim()}%", System.Data.DbType.String);
                }
                if (!string.IsNullOrWhiteSpace(filter.Address))
                {
                    whereBuilder.Append(" AND Address like @Address ");
                    parameters.Add("Address", $"%{filter.Address.Trim()}%", System.Data.DbType.String);
                }
                if (!string.IsNullOrWhiteSpace(filter.Email))
                {
                    whereBuilder.Append(" AND Email = @Email ");
                    parameters.Add("Email", filter.Email.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filter.Hotline))
                {
                    whereBuilder.Append(" AND Hotline = @Hotline ");
                    parameters.Add("Hotline", filter.Hotline.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filter.TaxCode))
                {
                    whereBuilder.Append(" AND TaxCode = @TaxCode ");
                    parameters.Add("TaxCode", filter.TaxCode.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filter.Representative))
                {
                    whereBuilder.Append(" AND Representative like @Representative ");
                    parameters.Add("Representative", $"%{filter.Representative.Trim()}%", System.Data.DbType.String);
                }
                if (!string.IsNullOrWhiteSpace(filter.RepresentativeIdNumber))
                {
                    whereBuilder.Append(" AND RepresentativeIdNumber = @RepresentativeIdNumber ");
                    parameters.Add("RepresentativeIdNumber", filter.RepresentativeIdNumber.Trim());
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
                string countSql = $"SELECT COUNT(1) FROM tbl_Site {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<SiteOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<SiteOutputDTO>()
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

                string querySql = $"SELECT * FROM tbl_Site {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var sites = await _dbContext.QueryAsync<SiteEntity>(querySql, parameters, cancellationToken);

                filterOutput.Records = sites.Select(s => _mapper.Map<SiteOutputDTO>(s)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteSite(DeleteSiteInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteSite-01] SiteId is required.", HttpStatusCode.BadRequest);
                }
                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_Site
                        Set Status = -1,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where SiteId = @SiteId
                        ", new
                    {
                        SiteId = input.SiteId,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId
                    },
                        cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteSite-02] Site with SiteId '{input.SiteId}' not found.", HttpStatusCode.NotFound);
                    }
                    return input.SiteId;
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_Site
                        Where SiteId = @SiteId
                        ", new
                    {
                        SiteId = input.SiteId
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteSite-02] Site with SiteId '{input.SiteId}' not found.", HttpStatusCode.NotFound);
                    }
                    return input.SiteId;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<SiteOutputDTO>> ImportSites(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var sites = await FileHelper.ReadFile<SiteInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<SiteInputDTO>());

            if (!sites.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempSites
                    (
                        SiteId NVARCHAR(20),
                        SiteName NVARCHAR(255),
                        SiteLogo VARBINARY(MAX),
                        Address NVARCHAR(MAX),
                        Email VARCHAR(255),
                        Hotline VARCHAR(20),
                        TaxCode VARCHAR(50),
                        Representative NVARCHAR(MAX),
                        RepresentativeIdNumber VARCHAR(12),
                        Status INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    sites,
                    "#TempSites",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_Site AS T
                    USING #TempSites AS S
                    ON T.SiteId = S.SiteId

                    WHEN MATCHED THEN
                        UPDATE SET
                            T.SiteName = S.SiteName,
                            T.SiteLogo = S.SiteLogo,
                            T.Address = S.Address,
                            T.Email = S.Email,
                            T.Hotline = S.Hotline,
                            T.TaxCode = S.TaxCode,
                            T.Representative = S.Representative,
                            T.RepresentativeIdNumber = S.RepresentativeIdNumber,
                            T.Status = S.Status,
                            T.UpdatedDate = GETUTCDATE(),
                            T.UpdatedBy = @UpdatedBy,
                            T.DeviceName = @DeviceName,
                            T.DeviceId = @DeviceId

                    WHEN NOT MATCHED THEN
                        INSERT
                        (
                            SiteId,
                            SiteName,
                            SiteLogo,
                            Address,
                            Email,
                            Hotline,
                            TaxCode,
                            Representative,
                            RepresentativeIdNumber,
                            Status,
                            CreatedDate,
                            CreatedBy,
                            DeviceName,
                            DeviceId
                        )
                        VALUES
                        (
                            S.SiteId,
                            S.SiteName,
                            S.SiteLogo,
                            S.Address,
                            S.Email,
                            S.Hotline,
                            S.TaxCode,
                            S.Representative,
                            S.RepresentativeIdNumber,
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

                return _mapper.Map<List<SiteOutputDTO>>(sites);
            }, cancellationToken);
        }

        public async Task<string> ExportSites(CancellationToken cancellationToken)
        {
            try
            {
                var sites = await _dbContext.QueryAsync<SiteEntity>(
                    @"Select * From tbl_Site Order By AutoId Asc",
                    cancellationToken: cancellationToken);

                var records = sites.Select(s => _mapper.Map<SiteOutputDTO>(s)).ToList();

                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<SiteExportMap>();
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
        private async Task<bool> CheckSiteExists(string siteId, CancellationToken cancellationToken)
        {
            try
            {
                var site = await _dbContext.QueryFirstOrDefaultAsync<SiteEntity>(
                    @"Select * From tbl_Site
                    Where SiteId = @SiteId",
                    new { SiteId = siteId },
                    cancellationToken);

                return site != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private sealed class SiteExportMap : ClassMap<SiteOutputDTO>
        {
            public SiteExportMap()
            {
                Map(m => m.SiteId);
                Map(m => m.SiteName);
                Map(m => m.Address);
                Map(m => m.Email);
                Map(m => m.Hotline);
                Map(m => m.TaxCode);
                Map(m => m.Representative);
                Map(m => m.RepresentativeIdNumber);
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
