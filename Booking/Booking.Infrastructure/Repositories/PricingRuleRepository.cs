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
    public class PricingRuleRepository : BaseRepository<PricingRuleRepository>, IPricingRuleRepository
    {
        private readonly BookingDbContext _dbContext;
        private readonly string _errorPrefix = "PricingRule_Repo_";

        public PricingRuleRepository(
            IMapper mapper,
            ILogger<PricingRuleRepository> logger,
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

        public async Task<PricingRuleOutputDTO> CreatePricingRule(PricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}CreatePricingRule-01] Input is null.", HttpStatusCode.BadRequest);

            var field = await GetFieldOwnedBySite(input.FieldId, callerSiteId, cancellationToken, "CreatePricingRule-02");

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            var newAutoId = await _dbContext.ExecuteScalarAsync<long>(@"
                INSERT INTO PricingRule (FieldId, DayOfWeek, StartTime, EndTime, PricePerHour, CreatedDate, CreatedBy, DeviceName, DeviceId)
                OUTPUT INSERTED.AutoId
                VALUES (@FieldId, @DayOfWeek, @StartTime, @EndTime, @PricePerHour, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
            ", new
            {
                input.FieldId,
                input.DayOfWeek,
                input.StartTime,
                input.EndTime,
                input.PricePerHour,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = input.CreatedBy,
                DeviceName = input.DeviceName,
                DeviceId = input.DeviceId,
            }, cancellationToken);

            if (newAutoId <= 0)
                throw new AppException($"[{_errorPrefix}CreatePricingRule-03] Failed to create PricingRule.", HttpStatusCode.InternalServerError);

            input.AutoId = newAutoId;
            return _mapper.Map<PricingRuleOutputDTO>(input);
        }

        public async Task<PricingRuleOutputDTO> UpdatePricingRule(PricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}UpdatePricingRule-01] Input is null.", HttpStatusCode.BadRequest);

            var existing = await _dbContext.QueryFirstOrDefaultAsync<PricingRule>(
                @"SELECT * FROM PricingRule WHERE AutoId = @AutoId", new { input.AutoId }, cancellationToken);
            if (existing == null)
                throw new AppException($"[{_errorPrefix}UpdatePricingRule-02] PricingRule '{input.AutoId}' not found.", HttpStatusCode.NotFound);

            // Verify the caller owns the record being mutated (via its CURRENT Field's Site) — checking only
            // the new input.FieldId below would let a caller who owns *some* Field in their own Site overwrite
            // another Site's PricingRule by AutoId, since AutoId alone carries no Site information.
            await GetFieldOwnedBySite(existing.FieldId, callerSiteId, cancellationToken, "UpdatePricingRule-03");

            // FieldId is mutable on this entity (unlike Field.SiteId in Task 5) — if it's being changed, the
            // NEW Field must also belong to the caller's Site, or a caller could reassign an owned rule onto
            // a foreign Site's Field.
            if (input.FieldId != existing.FieldId)
            {
                await GetFieldOwnedBySite(input.FieldId, callerSiteId, cancellationToken, "UpdatePricingRule-04");
            }

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            int rowsAffected = await _dbContext.ExecuteAsync(@"
                UPDATE PricingRule
                SET FieldId = @FieldId, DayOfWeek = @DayOfWeek, StartTime = @StartTime, EndTime = @EndTime,
                    PricePerHour = @PricePerHour, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy,
                    DeviceName = @DeviceName, DeviceId = @DeviceId
                WHERE AutoId = @AutoId
            ", new
            {
                input.AutoId,
                input.FieldId,
                input.DayOfWeek,
                input.StartTime,
                input.EndTime,
                input.PricePerHour,
                input.UpdatedDate,
                input.UpdatedBy,
                input.DeviceName,
                input.DeviceId,
            }, cancellationToken);

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}UpdatePricingRule-05] Failed to update PricingRule.", HttpStatusCode.InternalServerError);

            return _mapper.Map<PricingRuleOutputDTO>(input);
        }

        public async Task<PricingRuleOutputDTO> GetPricingRuleById(long autoId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.QueryFirstOrDefaultAsync<PricingRule>(
                @"SELECT * FROM PricingRule WHERE AutoId = @AutoId", new { AutoId = autoId }, cancellationToken);

            if (entity == null)
                throw new AppException($"[{_errorPrefix}GetPricingRuleById-01] PricingRule '{autoId}' not found.", HttpStatusCode.NotFound);

            return _mapper.Map<PricingRuleOutputDTO>(entity);
        }

        public async Task<BaseFilterOutput<PricingRuleOutputDTO>> GetAllPricingRules(PricingRuleFilterDTO filter, CancellationToken cancellationToken)
        {
            var whereBuilder = new StringBuilder(" WHERE 1=1 ");
            var parameters = new DynamicParameters();

            if (filter.Status.HasValue)
            {
                whereBuilder.Append(" AND [Status] = @Status ");
                parameters.Add("Status", filter.Status.Value);
            }
            if (!string.IsNullOrWhiteSpace(filter.FieldId))
            {
                whereBuilder.Append(" AND FieldId = @FieldId ");
                parameters.Add("FieldId", filter.FieldId);
            }
            if (filter.DayOfWeek.HasValue)
            {
                whereBuilder.Append(" AND DayOfWeek = @DayOfWeek ");
                parameters.Add("DayOfWeek", filter.DayOfWeek.Value);
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

            string countSql = $"SELECT COUNT(1) FROM PricingRule {whereBuilder}";
            var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

            var filterOutput = new BaseFilterOutput<PricingRuleOutputDTO>
            {
                TotalRecords = totalCount,
                TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Records = new List<PricingRuleOutputDTO>()
            };

            if (totalCount == 0)
                return filterOutput;

            string orderByClause = filter.SortBy switch
            {
                Sort.AutoIdDes => " ORDER BY AutoId DESC ",
                _ => " ORDER BY AutoId ASC "
            };

            string querySql = $"SELECT * FROM PricingRule {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
            parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

            var entities = await _dbContext.QueryAsync<PricingRule>(querySql, parameters, cancellationToken);
            filterOutput.Records = entities.Select(e => _mapper.Map<PricingRuleOutputDTO>(e)).ToList();

            return filterOutput;
        }

        public async Task<string> DeletePricingRule(DeletePricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken)
        {
            if (input == null || input.AutoId <= 0)
                throw new AppException($"[{_errorPrefix}DeletePricingRule-01] AutoId is required.", HttpStatusCode.BadRequest);

            var existing = await _dbContext.QueryFirstOrDefaultAsync<PricingRule>(
                @"SELECT * FROM PricingRule WHERE AutoId = @AutoId", new { input.AutoId }, cancellationToken);
            if (existing == null)
                throw new AppException($"[{_errorPrefix}DeletePricingRule-02] PricingRule '{input.AutoId}' not found.", HttpStatusCode.NotFound);

            await GetFieldOwnedBySite(existing.FieldId, callerSiteId, cancellationToken, "DeletePricingRule-03");

            int rowsAffected;
            if (input.IsSoftDelete)
            {
                rowsAffected = await _dbContext.ExecuteAsync(@"
                    UPDATE PricingRule SET Status = -1, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy
                    WHERE AutoId = @AutoId
                ", new { input.AutoId, UpdatedDate = DateTime.UtcNow, UpdatedBy = _auditContext.UserName }, cancellationToken);
            }
            else
            {
                rowsAffected = await _dbContext.ExecuteAsync(
                    @"DELETE FROM PricingRule WHERE AutoId = @AutoId", new { input.AutoId }, cancellationToken);
            }

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}DeletePricingRule-04] Failed to delete PricingRule.", HttpStatusCode.InternalServerError);

            return input.AutoId.ToString();
        }

        #endregion

        #region Helpers

        /// <summary>Loads the Field for <paramref name="fieldId"/> and throws unless it belongs to <paramref name="callerSiteId"/> — mirrors the check in FieldRepository.UpdateField/DeleteField, applied here so pricing can't be set on another Site's Field.</summary>
        private async Task<Field> GetFieldOwnedBySite(string fieldId, string callerSiteId, CancellationToken cancellationToken, string errorStep)
        {
            var field = await _dbContext.QueryFirstOrDefaultAsync<Field>(
                @"SELECT * FROM Field WHERE FieldId = @FieldId", new { FieldId = fieldId }, cancellationToken);

            if (field == null)
                throw new AppException($"[{_errorPrefix}{errorStep}] Field with Id '{fieldId}' not found.", HttpStatusCode.BadRequest);

            if (field.SiteId != callerSiteId)
                throw new AppException($"[{_errorPrefix}{errorStep}] Access denied. This Field does not belong to your Site.", HttpStatusCode.Forbidden);

            return field;
        }

        #endregion
    }
}
