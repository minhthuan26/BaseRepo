using AutoMapper;
using CommonLibs.DTOs;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Services;
using CommonLibs.Utils;
using Dapper;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;
using Booking.Domain.Entities;
using Booking.Infrastructure.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;
using System.Text;

namespace Booking.Infrastructure.Repositories
{
    public class FieldRepository : BaseRepository<FieldRepository>, IFieldRepository
    {
        private readonly BookingDbContext _dbContext;
        private readonly string _errorPrefix = "Field_Repo_";

        public FieldRepository(
            IMapper mapper,
            ILogger<FieldRepository> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContext,
            BookingDbContext dbContext,
            IOptions<AppOptions> options,
            IAuditContext auditContext,
            IJwtTokenService jwtTokenService)
            : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Repository Methods

        public async Task<FieldOutputDTO> CreateField(FieldInputDTO input, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}CreateField-01] Input is null.", HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(input.FieldId))
                throw new AppException($"[{_errorPrefix}CreateField-02] FieldId is required.", HttpStatusCode.BadRequest);

            if (await CheckFieldExists(input.FieldId, cancellationToken))
                throw new AppException($"[{_errorPrefix}CreateField-03] Field with Id '{input.FieldId}' already exists.", HttpStatusCode.Conflict);

            var fieldType = await _dbContext.QueryFirstOrDefaultAsync<FieldType>(
                @"SELECT * FROM FieldType WHERE FieldTypeId = @FieldTypeId",
                new { input.FieldTypeId }, cancellationToken);
            if (fieldType == null)
                throw new AppException($"[{_errorPrefix}CreateField-04] FieldType with Id '{input.FieldTypeId}' not found.", HttpStatusCode.BadRequest);

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            int rowsAffected = await _dbContext.ExecuteAsync(@"
                INSERT INTO Field (FieldId, SiteId, FieldTypeId, FieldName, OperationalStatus, CreatedDate, CreatedBy, DeviceName, DeviceId)
                VALUES (@FieldId, @SiteId, @FieldTypeId, @FieldName, @OperationalStatus, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
            ", new
            {
                input.FieldId,
                input.SiteId,
                input.FieldTypeId,
                input.FieldName,
                input.OperationalStatus,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = input.CreatedBy,
                DeviceName = input.DeviceName,
                DeviceId = input.DeviceId,
            }, cancellationToken);

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}CreateField-05] Failed to create Field.", HttpStatusCode.InternalServerError);

            return _mapper.Map<FieldOutputDTO>(input);
        }

        public async Task<FieldOutputDTO> UpdateField(FieldInputDTO input, string callerSiteId, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}UpdateField-01] Input is null.", HttpStatusCode.BadRequest);

            var existing = await _dbContext.QueryFirstOrDefaultAsync<Field>(
                @"SELECT * FROM Field WHERE FieldId = @FieldId", new { input.FieldId }, cancellationToken);
            if (existing == null)
                throw new AppException($"[{_errorPrefix}UpdateField-02] Field with Id '{input.FieldId}' not found.", HttpStatusCode.NotFound);

            if (existing.SiteId != callerSiteId)
                throw new AppException($"[{_errorPrefix}UpdateField-03] Access denied. This Field does not belong to your Site.", HttpStatusCode.Forbidden);

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            int rowsAffected = await _dbContext.ExecuteAsync(@"
                UPDATE Field
                SET FieldTypeId = @FieldTypeId, FieldName = @FieldName, OperationalStatus = @OperationalStatus,
                    UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy, DeviceName = @DeviceName, DeviceId = @DeviceId
                WHERE FieldId = @FieldId
            ", new
            {
                input.FieldId,
                input.FieldTypeId,
                input.FieldName,
                input.OperationalStatus,
                input.UpdatedDate,
                input.UpdatedBy,
                input.DeviceName,
                input.DeviceId,
            }, cancellationToken);

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}UpdateField-04] Failed to update Field.", HttpStatusCode.InternalServerError);

            input.SiteId = existing.SiteId;
            return _mapper.Map<FieldOutputDTO>(input);
        }

        public async Task<FieldOutputDTO> GetFieldById(string fieldId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.QueryFirstOrDefaultAsync<Field>(
                @"SELECT * FROM Field WHERE FieldId = @FieldId", new { FieldId = fieldId }, cancellationToken);

            if (entity == null)
                throw new AppException($"[{_errorPrefix}GetFieldById-01] Field with Id '{fieldId}' not found.", HttpStatusCode.NotFound);

            return _mapper.Map<FieldOutputDTO>(entity);
        }

        public async Task<BaseFilterOutput<FieldOutputDTO>> GetAllFields(FieldFilterDTO filter, CancellationToken cancellationToken)
        {
            var whereBuilder = new StringBuilder(" WHERE 1=1 ");
            var parameters = new DynamicParameters();

            if (filter.Status.HasValue)
            {
                whereBuilder.Append(" AND [Status] = @Status ");
                parameters.Add("Status", filter.Status.Value);
            }
            if (!string.IsNullOrWhiteSpace(filter.FieldName))
            {
                whereBuilder.Append(" AND FieldName LIKE @FieldName ");
                parameters.Add("FieldName", $"%{filter.FieldName.Trim()}%", DbType.String);
            }
            if (!string.IsNullOrWhiteSpace(filter.FieldTypeId))
            {
                whereBuilder.Append(" AND FieldTypeId = @FieldTypeId ");
                parameters.Add("FieldTypeId", filter.FieldTypeId);
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

            string countSql = $"SELECT COUNT(1) FROM Field {whereBuilder}";
            var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

            var filterOutput = new BaseFilterOutput<FieldOutputDTO>
            {
                TotalRecords = totalCount,
                TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Records = new List<FieldOutputDTO>()
            };

            if (totalCount == 0)
                return filterOutput;

            string orderByClause = filter.SortBy switch
            {
                Sort.AutoIdDes => " ORDER BY AutoId DESC ",
                _ => " ORDER BY AutoId ASC "
            };

            string querySql = $"SELECT * FROM Field {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
            parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

            var entities = await _dbContext.QueryAsync<Field>(querySql, parameters, cancellationToken);
            filterOutput.Records = entities.Select(e => _mapper.Map<FieldOutputDTO>(e)).ToList();

            return filterOutput;
        }

        public async Task<string> DeleteField(DeleteFieldInputDTO input, string callerSiteId, CancellationToken cancellationToken)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.FieldId))
                throw new AppException($"[{_errorPrefix}DeleteField-01] FieldId is required.", HttpStatusCode.BadRequest);

            var existing = await _dbContext.QueryFirstOrDefaultAsync<Field>(
                @"SELECT * FROM Field WHERE FieldId = @FieldId", new { input.FieldId }, cancellationToken);
            if (existing == null)
                throw new AppException($"[{_errorPrefix}DeleteField-02] Field with Id '{input.FieldId}' not found.", HttpStatusCode.NotFound);

            if (existing.SiteId != callerSiteId)
                throw new AppException($"[{_errorPrefix}DeleteField-03] Access denied. This Field does not belong to your Site.", HttpStatusCode.Forbidden);

            int rowsAffected;
            if (input.IsSoftDelete)
            {
                rowsAffected = await _dbContext.ExecuteAsync(@"
                    UPDATE Field SET Status = -1, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy
                    WHERE FieldId = @FieldId
                ", new { input.FieldId, UpdatedDate = DateTime.UtcNow, UpdatedBy = _auditContext.UserName }, cancellationToken);
            }
            else
            {
                rowsAffected = await _dbContext.ExecuteAsync(
                    @"DELETE FROM Field WHERE FieldId = @FieldId", new { input.FieldId }, cancellationToken);
            }

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}DeleteField-04] Failed to delete Field.", HttpStatusCode.InternalServerError);

            return input.FieldId!;
        }

        #endregion

        #region Helpers

        private async Task<bool> CheckFieldExists(string fieldId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.QueryFirstOrDefaultAsync<Field>(
                @"SELECT * FROM Field WHERE FieldId = @FieldId", new { FieldId = fieldId }, cancellationToken);

            return entity != null;
        }

        #endregion
    }
}
