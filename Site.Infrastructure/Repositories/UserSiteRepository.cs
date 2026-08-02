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
using SiteEntity = Site.Domain.Entities.Site;
using UserSiteEntity = Site.Domain.Entities.UserSite;

namespace Site.Infrastructure.Repositories
{
    public class UserSiteRepository : BaseRepository<UserSiteRepository>, IUserSiteRepository
    {
        private readonly SiteDbContext _dbContext;
        private readonly string _errorPrefix = "UserSite_Repo_";

        public UserSiteRepository(IMapper mapper, ILogger<UserSiteRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<UserSiteOutputDTO> CreateUserSite(UserSiteInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateUserSite-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.UserId))
                {
                    throw new AppException($"[{_errorPrefix}CreateUserSite-02] UserId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}CreateUserSite-03] SiteId is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckUserSiteExists(input.UserId, input.SiteId, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateUserSite-04] UserSite with UserId '{input.UserId}' and SiteId '{input.SiteId}' already exists.", HttpStatusCode.Conflict);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into tbl_User_Site (UserId, SiteId, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@UserId, @SiteId, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    UserId = input.UserId,
                    SiteId = input.SiteId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateUserSite-05] Failed to create user site.", HttpStatusCode.InternalServerError);
                }
                return _mapper.Map<UserSiteOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<UserSiteOutputDTO> GetUserSiteById(string userId, string siteId, CancellationToken cancellationToken)
        {
            try
            {
                var userSite = await _dbContext.QueryFirstOrDefaultAsync<UserSiteEntity>(
                    @"Select * From tbl_User_Site
                    Where UserId = @UserId And SiteId = @SiteId",
                    new { UserId = userId, SiteId = siteId },
                    cancellationToken);
                if (userSite == null)
                {
                    throw new AppException($"[{_errorPrefix}GetUserSiteById-01] UserSite with UserId '{userId}' and SiteId '{siteId}' not found.", HttpStatusCode.NotFound);
                }
                return _mapper.Map<UserSiteOutputDTO>(userSite);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<UserSiteOutputDTO>> GetAllUserSites(UserSiteFilterDTO filter, CancellationToken cancellationToken)
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
                if (!string.IsNullOrWhiteSpace(filter.UserId))
                {
                    whereBuilder.Append(" AND UserId = @UserId ");
                    parameters.Add("UserId", filter.UserId.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filter.SiteId))
                {
                    whereBuilder.Append(" AND SiteId = @SiteId ");
                    parameters.Add("SiteId", filter.SiteId.Trim());
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
                string countSql = $"SELECT COUNT(1) FROM tbl_User_Site {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<UserSiteOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<UserSiteOutputDTO>()
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

                string querySql = $"SELECT * FROM tbl_User_Site {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var userSites = await _dbContext.QueryAsync<UserSiteEntity>(querySql, parameters, cancellationToken);

                filterOutput.Records = userSites.Select(us => _mapper.Map<UserSiteOutputDTO>(us)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteUserSite(DeleteUserSiteInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.UserId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteUserSite-01] UserId is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.SiteId))
                {
                    throw new AppException($"[{_errorPrefix}DeleteUserSite-02] SiteId is required.", HttpStatusCode.BadRequest);
                }
                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update tbl_User_Site
                        Set Status = -1,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where UserId = @UserId And SiteId = @SiteId
                        ", new
                    {
                        UserId = input.UserId,
                        SiteId = input.SiteId,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId
                    },
                        cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteUserSite-03] UserSite with UserId '{input.UserId}' and SiteId '{input.SiteId}' not found.", HttpStatusCode.NotFound);
                    }
                    return $"{input.UserId}/{input.SiteId}";
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From tbl_User_Site
                        Where UserId = @UserId And SiteId = @SiteId
                        ", new
                    {
                        UserId = input.UserId,
                        SiteId = input.SiteId
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteUserSite-03] UserSite with UserId '{input.UserId}' and SiteId '{input.SiteId}' not found.", HttpStatusCode.NotFound);
                    }
                    return $"{input.UserId}/{input.SiteId}";
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<UserSiteOutputDTO>> ImportUserSites(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var userSites = await FileHelper.ReadFile<UserSiteInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<UserSiteInputDTO>());

            if (!userSites.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempUserSites
                    (
                        UserId NVARCHAR(20),
                        SiteId NVARCHAR(20),
                        Status INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    userSites,
                    "#TempUserSites",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE tbl_User_Site AS T
                    USING #TempUserSites AS S
                    ON T.UserId = S.UserId AND T.SiteId = S.SiteId

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
                            S.SiteId,
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

                return _mapper.Map<List<UserSiteOutputDTO>>(userSites);
            }, cancellationToken);
        }

        public async Task<string> ExportUserSites(CancellationToken cancellationToken)
        {
            try
            {
                var userSites = await _dbContext.QueryAsync<UserSiteEntity>(
                    @"Select * From tbl_User_Site Order By AutoId Asc",
                    cancellationToken: cancellationToken);

                var records = userSites.Select(us => _mapper.Map<UserSiteOutputDTO>(us)).ToList();

                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<UserSiteExportMap>();
                    await csvWriter.WriteRecordsAsync(records, cancellationToken);
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<SiteOutputDTO>> GetSitesByUser(string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new AppException($"[{_errorPrefix}GetSitesByUser-01] UserId is required.", HttpStatusCode.BadRequest);
                }

                var sites = await _dbContext.QueryAsync<SiteEntity>(@"
                    Select S.* From tbl_Site S
                    Inner Join tbl_User_Site US On US.SiteId = S.SiteId
                    Where US.UserId = @UserId And US.Status = 1 And S.Status = 1
                    Order By S.AutoId Asc",
                    new { UserId = userId },
                    cancellationToken);

                return sites.Select(s => _mapper.Map<SiteOutputDTO>(s)).ToList();
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Helpers
        private async Task<bool> CheckUserSiteExists(string userId, string siteId, CancellationToken cancellationToken)
        {
            try
            {
                var userSite = await _dbContext.QueryFirstOrDefaultAsync<UserSiteEntity>(
                    @"Select * From tbl_User_Site
                    Where UserId = @UserId And SiteId = @SiteId",
                    new { UserId = userId, SiteId = siteId },
                    cancellationToken);

                return userSite != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private sealed class UserSiteExportMap : ClassMap<UserSiteOutputDTO>
        {
            public UserSiteExportMap()
            {
                Map(m => m.UserId);
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
