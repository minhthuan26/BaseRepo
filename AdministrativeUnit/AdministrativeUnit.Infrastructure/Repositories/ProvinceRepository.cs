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
using ProvinceEntity = AdministrativeUnit.Domain.Entities.Province;

namespace AdministrativeUnit.Infrastructure.Repositories
{
    public class ProvinceRepository : BaseRepository<ProvinceRepository>, IProvinceRepository
    {
        private readonly AdministrativeUnitDbContext _dbContext;
        private readonly string _errorPrefix = "Province_Repo_";

        public ProvinceRepository(IMapper mapper, ILogger<ProvinceRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, AdministrativeUnitDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Stored Procedure

        #endregion

        #region Repository Methods
        public async Task<ProvinceOutputDTO> CreateProvince(ProvinceInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}CreateProvince-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.Code))
                {
                    throw new AppException($"[{_errorPrefix}CreateProvince-02] Code is required.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.Name))
                {
                    throw new AppException($"[{_errorPrefix}CreateProvince-03] Name is required.", HttpStatusCode.BadRequest);
                }
                if (await CheckProvinceExists(input.Code, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}CreateProvince-04] Province with Code '{input.Code}' already exists.", HttpStatusCode.Conflict);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);
                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Insert Into Province (Code, Name, Type, NameWithType, Slug, CreatedDate, CreatedBy, DeviceName, DeviceId)
                        Values (@Code, @Name, @Type, @NameWithType, @Slug, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                    ", new
                {
                    Code = input.Code,
                    Name = input.Name,
                    Type = input.Type,
                    NameWithType = input.NameWithType,
                    Slug = input.Slug,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                },
                    cancellationToken);
                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}CreateProvince-05] Failed to create province.", HttpStatusCode.InternalServerError);
                }
                return _mapper.Map<ProvinceOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ProvinceOutputDTO> UpdateProvince(ProvinceInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                {
                    throw new AppException($"[{_errorPrefix}UpdateProvince-01] Input is null.", HttpStatusCode.BadRequest);
                }
                if (string.IsNullOrWhiteSpace(input.Code))
                {
                    throw new AppException($"[{_errorPrefix}UpdateProvince-02] Code is required.", HttpStatusCode.BadRequest);
                }
                if (!await CheckProvinceExists(input.Code, cancellationToken))
                {
                    throw new AppException($"[{_errorPrefix}UpdateProvince-03] Province with Code '{input.Code}' not found.", HttpStatusCode.NotFound);
                }

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

                int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update Province
                        Set Name = @Name,
                            Type = @Type,
                            NameWithType = @NameWithType,
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
                    Slug = input.Slug,
                    Status = input.Status.HasValue ? (int)input.Status.Value : 1,
                    UpdatedDate = DateTime.UtcNow,
                    UpdatedBy = input.UpdatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                }, cancellationToken);

                if (rowsAffected <= 0)
                {
                    throw new AppException($"[{_errorPrefix}UpdateProvince-04] Failed to update province.", HttpStatusCode.InternalServerError);
                }

                return _mapper.Map<ProvinceOutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ProvinceOutputDTO> GetProvinceById(string code, CancellationToken cancellationToken)
        {
            try
            {
                var province = await _dbContext.QueryFirstOrDefaultAsync<ProvinceEntity>(
                    @"Select * From Province
                    Where Code = @Code",
                    new { Code = code },
                    cancellationToken);
                if (province == null)
                {
                    throw new AppException($"[{_errorPrefix}GetProvinceById-01] Province with Code '{code}' not found.", HttpStatusCode.NotFound);
                }
                return _mapper.Map<ProvinceOutputDTO>(province);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<ProvinceOutputDTO>> GetAllProvinces(ProvinceFilterDTO filter, CancellationToken cancellationToken)
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
                if (!string.IsNullOrWhiteSpace(filter.Type))
                {
                    whereBuilder.Append(" AND [Type] = @Type ");
                    parameters.Add("Type", filter.Type.Trim());
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
                string countSql = $"SELECT COUNT(1) FROM Province {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<ProvinceOutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<ProvinceOutputDTO>()
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

                string querySql = $"SELECT * FROM Province {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var provinces = await _dbContext.QueryAsync<ProvinceEntity>(querySql, parameters, cancellationToken);

                filterOutput.Records = provinces.Select(p => _mapper.Map<ProvinceOutputDTO>(p)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteProvince(DeleteProvinceInputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.Code))
                {
                    throw new AppException($"[{_errorPrefix}DeleteProvince-01] Code is required.", HttpStatusCode.BadRequest);
                }
                if (input.IsSoftDelete)
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Update Province
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
                        throw new AppException($"[{_errorPrefix}DeleteProvince-02] Province with Code '{input.Code}' not found.", HttpStatusCode.NotFound);
                    }
                    return input.Code;
                }
                else
                {
                    int rowsAffected = await _dbContext.ExecuteAsync(@"
                        Delete From Province
                        Where Code = @Code
                        ", new
                    {
                        Code = input.Code
                    },
                    cancellationToken);
                    if (rowsAffected <= 0)
                    {
                        throw new AppException($"[{_errorPrefix}DeleteProvince-02] Province with Code '{input.Code}' not found.", HttpStatusCode.NotFound);
                    }
                    return input.Code;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ProvinceOutputDTO>> ImportProvinces(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            var provinces = await FileHelper.ReadFile<ProvinceInputDTO>(stream, fileName, BaseInputDTO.GetHeaders<ProvinceInputDTO>());

            if (!provinces.Any())
                return new();

            return await _dbContext.ExecuteWithTransactionAsync(async () =>
            {
                await _dbContext.ExecuteAsync(@"
                    CREATE TABLE #TempProvinces
                    (
                        Code VARCHAR(20),
                        Name NVARCHAR(100),
                        Type NVARCHAR(50),
                        NameWithType NVARCHAR(150),
                        Slug VARCHAR(150),
                        Status INT
                    )
                    ", cancellationToken: cancellationToken);

                await _dbContext.ExecuteBulkAsync(
                    provinces,
                    "#TempProvinces",
                    cancellationToken: cancellationToken);

                await _dbContext.ExecuteAsync(@"
                    MERGE Province AS T
                    USING #TempProvinces AS S
                    ON T.Code = S.Code

                    WHEN MATCHED THEN
                        UPDATE SET
                            T.Name = S.Name,
                            T.Type = S.Type,
                            T.NameWithType = S.NameWithType,
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

                return _mapper.Map<List<ProvinceOutputDTO>>(provinces);
            }, cancellationToken);
        }

        public async Task<string> ExportProvinces(CancellationToken cancellationToken)
        {
            try
            {
                var provinces = await _dbContext.QueryAsync<ProvinceEntity>(
                    @"Select * From Province Order By AutoId Asc",
                    cancellationToken: cancellationToken);

                var records = provinces.Select(p => _mapper.Map<ProvinceOutputDTO>(p)).ToList();

                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<ProvinceExportMap>();
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
        private async Task<bool> CheckProvinceExists(string code, CancellationToken cancellationToken)
        {
            try
            {
                var province = await _dbContext.QueryFirstOrDefaultAsync<ProvinceEntity>(
                    @"Select * From Province
                    Where Code = @Code",
                    new { Code = code },
                    cancellationToken);

                return province != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private sealed class ProvinceExportMap : ClassMap<ProvinceOutputDTO>
        {
            public ProvinceExportMap()
            {
                Map(m => m.Code);
                Map(m => m.Name);
                Map(m => m.Type);
                Map(m => m.NameWithType);
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
