using AdministrativeUnit.Application.Interfaces;
using AdministrativeUnit.Contract.DTOs;
using AdministrativeUnit.Infrastructure.DBContext;
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
using System.Globalization;
using System.Net;
using System.Text;
using WardEntity = AdministrativeUnit.Domain.Entities.Ward;

namespace AdministrativeUnit.Infrastructure.Repositories
{
    public class WardRepository : BaseRepository<WardRepository>, IWardRepository
    {
        private readonly AdministrativeUnitDbContext _dbContext;
        private readonly string _errorPrefix = "Ward_Repo_";

        public WardRepository(IMapper mapper, ILogger<WardRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, AdministrativeUnitDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<WardOutputDTO> CreateWard(WardInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateWard-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.Code))
                {
                    throw new AppException($"[{_errorPrefix}CreateWard-02] Code is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.Name))
                {
                    throw new AppException($"[{_errorPrefix}CreateWard-03] Name is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.ProvinceCode))
                {
                    throw new AppException($"[{_errorPrefix}CreateWard-04] ProvinceCode is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckWardExists(input.Code, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateWard-05] Ward with Code '{input.Code}' already exists.", HttpStatusCode.Conflict);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into Ward (Code, Name, Type, NameWithType, ProvinceCode, Slug, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@Code, @Name, @Type, @NameWithType, @ProvinceCode, @Slug, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    Code = input.Code,
                    Name = input.Name,
                    Type = input.Type,
                    NameWithType = input.NameWithType,
                    ProvinceCode = input.ProvinceCode,
                    Slug = input.Slug,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateWard-06] Failed to create ward.", HttpStatusCode.InternalServerError);
                }
                return _mapper.Map<WardOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<WardOutputDTO> UpdateWard(WardInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}UpdateWard-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.Code))
                {
                    throw new AppException($"[{_errorPrefix}UpdateWard-02] Code is required.", HttpStatusCode.BadRequest);
                }
                if (!await CheckWardExists(input.Code, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}UpdateWard-03] Ward with Code '{input.Code}' not found.", HttpStatusCode.NotFound);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update Ward
                        Set Name = @Name,
                            Type = @Type,
                            NameWithType = @NameWithType,
                            ProvinceCode = @ProvinceCode,
                            Slug = @Slug,
                            Status = @Status,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where Code = @Code
                    ", new
                {
                    Code = input.Code,
                    Name = input.Name,
                    Type = input.Type,
                    NameWithType = input.NameWithType,
                    ProvinceCode = input.ProvinceCode,
                    Slug = input.Slug,
                    Status = input.Status.HasValue ? (int)input.Status.Value : 1,
                    UpdatedDate = DateTime.UtcNow,
                    UpdatedBy = input.UpdatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                }, cancellationToken);

                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}UpdateWard-04] Failed to update ward.", HttpStatusCode.InternalServerError);
                }

                return _mapper.Map<WardOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<WardOutputDTO> GetWardById(string code, CancellationToken cancellationToken)
        {
            try
            {
                var ward = await _dbContext.QueryFirstOrDefaultAsync<WardEntity>(
                    @"Select * From Ward
                    Where Code = @Code",
                    new { Code = code },
                    cancellationToken);
                if (ward == null)
                {
                    throw new AppException($"[{_errorPrefix}GetWardById-01] Ward with Code '{code}' not found.", HttpStatusCode.NotFound);
                }
                return _mapper.Map<WardOutputDTO>(ward);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<WardOutputDTO>> GetAllWards(WardFilterDTO filter, CancellationToken cancellationToken)
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
                if (!string.IsNullOrWhiteSpace(filter.Name))
                {
                    whereBuilder.Append(" AND Name like @Name ");
                    parameters.Add("Name", $"%{filter.Name.Trim()}%", System.Data.DbType.String);
                }
                if (!string.IsNullOrWhiteSpace(filter.ProvinceCode))
                {
                    whereBuilder.Append(" AND ProvinceCode = @ProvinceCode ");
                    parameters.Add("ProvinceCode", filter.ProvinceCode.Trim());
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
                string countSql = $"SELECT COUNT(1) FROM Ward {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<WardOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<WardOutputDTO>()
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

                string querySql = $"SELECT * FROM Ward {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var wards = await _dbContext.QueryAsync<WardEntity>(querySql, parameters, cancellationToken);

                filterOutput.Records = wards.Select(w => _mapper.Map<WardOutputDTO>(w)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteWard(DeleteWardInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.Code))
                {
                    throw new AppException($"[{_errorPrefix}DeleteWard-01] Code is required.", HttpStatusCode.BadRequest);
                }
                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update Ward
                        Set Status = -1,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy,
                            DeviceName = @DeviceName,
                            DeviceId = @DeviceId
                        Where Code = @Code
                        ", new
                    {
                        Code = input.Code,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = _auditContext.UserName,
                        DeviceName = _auditContext.DeviceName,
                        DeviceId = _auditContext.DeviceId
                    },
                        cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteWard-02] Ward with Code '{input.Code}' not found.", HttpStatusCode.NotFound);
                    }
                    return input.Code;
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From Ward
                        Where Code = @Code
                        ", new
                    {
                        Code = input.Code
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteWard-02] Ward with Code '{input.Code}' not found.", HttpStatusCode.NotFound);
                    }
                    return input.Code;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<WardOutputDTO>> ImportWards(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var wards = await FileHelper.ReadFile<WardInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<WardInputDTO>());

            if (!wards.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempWards
                    (
                        Code VARCHAR(20),
                        Name NVARCHAR(100),
                        Type NVARCHAR(50),
                        NameWithType NVARCHAR(150),
                        ProvinceCode VARCHAR(20),
                        Slug VARCHAR(150),
                        Status INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    wards,
                    "#TempWards",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE Ward AS T
                    USING #TempWards AS S
                    ON T.Code = S.Code

                    WHEN MATCHED THEN
                        UPDATE SET
                            T.Name = S.Name,
                            T.Type = S.Type,
                            T.NameWithType = S.NameWithType,
                            T.ProvinceCode = S.ProvinceCode,
                            T.Slug = S.Slug,
                            T.Status = S.Status,
                            T.UpdatedDate = GETUTCDATE(),
                            T.UpdatedBy = @UpdatedBy,
                            T.DeviceName = @DeviceName,
                            T.DeviceId = @DeviceId

                    WHEN NOT MATCHED THEN
                        INSERT
                        (
                            Code,
                            Name,
                            Type,
                            NameWithType,
                            ProvinceCode,
                            Slug,
                            Status,
                            CreatedDate,
                            CreatedBy,
                            DeviceName,
                            DeviceId
                        )
                        VALUES
                        (
                            S.Code,
                            S.Name,
                            S.Type,
                            S.NameWithType,
                            S.ProvinceCode,
                            S.Slug,
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

                return _mapper.Map<List<WardOutputDTO>>(wards);
            }, cancellationToken);
        }

        public async Task<string> ExportWards(CancellationToken cancellationToken)
        {
            try
            {
                var wards = await _dbContext.QueryAsync<WardEntity>(
                    @"Select * From Ward Order By AutoId Asc",
                    cancellationToken: cancellationToken);

                var records = wards.Select(w => _mapper.Map<WardOutputDTO>(w)).ToList();

                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<WardExportMap>();
                    await csvWriter.WriteRecordsAsync(records, cancellationToken);
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<WardOutputDTO>> GetWardsByProvince(string provinceCode, CancellationToken cancellationToken)
        {
            try
            {
                var wards = await _dbContext.QueryAsync<WardEntity>(
                    @"Select * From Ward
                    Where ProvinceCode = @ProvinceCode And Status = 1
                    Order By AutoId Asc",
                    new { ProvinceCode = provinceCode },
                    cancellationToken);

                return wards.Select(w => _mapper.Map<WardOutputDTO>(w)).ToList();
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Helpers
        private async Task<bool> CheckWardExists(string code, CancellationToken cancellationToken)
        {
            try
            {
                var ward = await _dbContext.QueryFirstOrDefaultAsync<WardEntity>(
                    @"Select * From Ward
                    Where Code = @Code",
                    new { Code = code },
                    cancellationToken);

                return ward != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private sealed class WardExportMap : ClassMap<WardOutputDTO>
        {
            public WardExportMap()
            {
                Map(m => m.Code);
                Map(m => m.Name);
                Map(m => m.Type);
                Map(m => m.NameWithType);
                Map(m => m.ProvinceCode);
                Map(m => m.Slug);
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
