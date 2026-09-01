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
    public class FieldTypeRepository : BaseRepository<FieldTypeRepository>, IFieldTypeRepository
    {
        private readonly BookingDbContext _dbContext;
        private readonly string _errorPrefix = "FieldType_Repo_";

        public FieldTypeRepository(
            IMapper mapper,
            ILogger<FieldTypeRepository> logger,
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

        public async Task<FieldTypeOutputDTO> CreateFieldType(FieldTypeInputDTO input, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}CreateFieldType-01] Input is null.", HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(input.FieldTypeId))
                throw new AppException($"[{_errorPrefix}CreateFieldType-02] FieldTypeId is required.", HttpStatusCode.BadRequest);

            if (await CheckFieldTypeExists(input.FieldTypeId, cancellationToken))
                throw new AppException($"[{_errorPrefix}CreateFieldType-03] FieldType with Id '{input.FieldTypeId}' already exists.", HttpStatusCode.Conflict);

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            int rowsAffected = await _dbContext.ExecuteAsync(@"
                INSERT INTO FieldType (FieldTypeId, FieldTypeName, CreatedDate, CreatedBy, DeviceName, DeviceId)
                VALUES (@FieldTypeId, @FieldTypeName, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
            ", new
            {
                input.FieldTypeId,
                input.FieldTypeName,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = input.CreatedBy,
                DeviceName = input.DeviceName,
                DeviceId = input.DeviceId,
            }, cancellationToken);

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}CreateFieldType-04] Failed to create FieldType.", HttpStatusCode.InternalServerError);

            return _mapper.Map<FieldTypeOutputDTO>(input);
        }

        public async Task<FieldTypeOutputDTO> UpdateFieldType(FieldTypeInputDTO input, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}UpdateFieldType-01] Input is null.", HttpStatusCode.BadRequest);

            if (!await CheckFieldTypeExists(input.FieldTypeId, cancellationToken))
                throw new AppException($"[{_errorPrefix}UpdateFieldType-02] FieldType with Id '{input.FieldTypeId}' not found.", HttpStatusCode.NotFound);

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            int rowsAffected = await _dbContext.ExecuteAsync(@"
                UPDATE FieldType
                SET FieldTypeName = @FieldTypeName, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy,
                    DeviceName = @DeviceName, DeviceId = @DeviceId
                WHERE FieldTypeId = @FieldTypeId
            ", new
            {
                input.FieldTypeId,
                input.FieldTypeName,
                input.UpdatedDate,
                input.UpdatedBy,
                input.DeviceName,
                input.DeviceId,
            }, cancellationToken);

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}UpdateFieldType-03] Failed to update FieldType.", HttpStatusCode.InternalServerError);

            return _mapper.Map<FieldTypeOutputDTO>(input);
        }

        public async Task<FieldTypeOutputDTO> GetFieldTypeById(string fieldTypeId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.QueryFirstOrDefaultAsync<FieldType>(
                @"SELECT * FROM FieldType WHERE FieldTypeId = @FieldTypeId",
                new { FieldTypeId = fieldTypeId },
                cancellationToken);

            if (entity == null)
                throw new AppException($"[{_errorPrefix}GetFieldTypeById-01] FieldType with Id '{fieldTypeId}' not found.", HttpStatusCode.NotFound);

            return _mapper.Map<FieldTypeOutputDTO>(entity);
        }

        public async Task<BaseFilterOutput<FieldTypeOutputDTO>> GetAllFieldTypes(FieldTypeFilterDTO filter, CancellationToken cancellationToken)
        {
            var whereBuilder = new StringBuilder(" WHERE 1=1 ");
            var parameters = new DynamicParameters();

            if (filter.Status.HasValue)
            {
                whereBuilder.Append(" AND [Status] = @Status ");
                parameters.Add("Status", filter.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.FieldTypeName))
            {
                whereBuilder.Append(" AND FieldTypeName LIKE @FieldTypeName ");
                parameters.Add("FieldTypeName", $"%{filter.FieldTypeName.Trim()}%", DbType.String);
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

            string countSql = $"SELECT COUNT(1) FROM FieldType {whereBuilder}";
            var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

            var filterOutput = new BaseFilterOutput<FieldTypeOutputDTO>
            {
                TotalRecords = totalCount,
                TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Records = new List<FieldTypeOutputDTO>()
            };

            if (totalCount == 0)
                return filterOutput;

            string orderByClause = filter.SortBy switch
            {
                Sort.AutoIdDes => " ORDER BY AutoId DESC ",
                _ => " ORDER BY AutoId ASC "
            };

            string querySql = $"SELECT * FROM FieldType {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
            parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

            var entities = await _dbContext.QueryAsync<FieldType>(querySql, parameters, cancellationToken);
            filterOutput.Records = entities.Select(e => _mapper.Map<FieldTypeOutputDTO>(e)).ToList();

            return filterOutput;
        }

        public async Task<string> DeleteFieldType(DeleteFieldTypeInputDTO input, CancellationToken cancellationToken)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.FieldTypeId))
                throw new AppException($"[{_errorPrefix}DeleteFieldType-01] FieldTypeId is required.", HttpStatusCode.BadRequest);

            if (!await CheckFieldTypeExists(input.FieldTypeId, cancellationToken))
                throw new AppException($"[{_errorPrefix}DeleteFieldType-02] FieldType with Id '{input.FieldTypeId}' not found.", HttpStatusCode.NotFound);

            int rowsAffected;
            if (input.IsSoftDelete)
            {
                rowsAffected = await _dbContext.ExecuteAsync(@"
                    UPDATE FieldType SET Status = -1, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy
                    WHERE FieldTypeId = @FieldTypeId
                ", new { input.FieldTypeId, UpdatedDate = DateTime.UtcNow, UpdatedBy = _auditContext.UserName }, cancellationToken);
            }
            else
            {
                var fieldCount = await _dbContext.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(1) FROM Field WHERE FieldTypeId = @FieldTypeId", new { input.FieldTypeId }, cancellationToken);
                if (fieldCount > 0)
                    throw new AppException($"[{_errorPrefix}DeleteFieldType-04] Cannot hard-delete FieldType '{input.FieldTypeId}': {fieldCount} Field row(s) still reference it. Remove them first or use soft-delete.", HttpStatusCode.Conflict);

                rowsAffected = await _dbContext.ExecuteAsync(
                    @"DELETE FROM FieldType WHERE FieldTypeId = @FieldTypeId",
                    new { input.FieldTypeId }, cancellationToken);
            }

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}DeleteFieldType-03] Failed to delete FieldType.", HttpStatusCode.InternalServerError);

            return input.FieldTypeId!;
        }

        #endregion

        #region Helpers

        private async Task<bool> CheckFieldTypeExists(string fieldTypeId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.QueryFirstOrDefaultAsync<FieldType>(
                @"SELECT * FROM FieldType WHERE FieldTypeId = @FieldTypeId",
                new { FieldTypeId = fieldTypeId },
                cancellationToken);

            return entity != null;
        }

        #endregion
    }
}
