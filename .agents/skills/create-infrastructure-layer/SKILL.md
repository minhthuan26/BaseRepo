---
name: create-infrastructure-layer
description: >
  Tạo Repository và cập nhật DependencyInjection trong tầng Infrastructure theo Clean Architecture cho BaseRepo project.
  Sử dụng khi cần implement Data Access logic (sử dụng Dapper / SQL queries) cho một entity.
---

# Skill: Tạo Infrastructure Layer

## Tổng quan

Tầng **Infrastructure** chịu trách nhiệm giao tiếp với Database, File System, External APIs,... Trong BaseRepo, tầng này chủ yếu chứa các class implement Interfaces từ Application layer (Repositories) và cấu hình Dependency Injection (DI).

### Thành phần cần tạo/cập nhật

| # | File | Mô tả |
|---|---|---|
| 1 | `{ServiceName}.Infrastructure/Repositories/{EntityName}Repository.cs` | Implement Data Access logic |
| 2 | `{ServiceName}.Infrastructure/DependencyInjection.cs` | Đăng ký Repository vào DI container |

---

## Quy tắc

1. **Repository** phải kế thừa `BaseRepository<{EntityName}Repository>` từ `CommonLibs.Repositories`
2. **Repository** phải implement `I{EntityName}Repository` từ Application layer
3. Sử dụng **Dapper** (các extension methods từ Dapper) cho raw SQL queries (`ExecuteAsync`, `QueryFirstOrDefaultAsync`, `QueryAsync`, `ExecuteScalarAsync`)
4. Exception cần throw bằng `AppException` với prefix `[{EntityName}_Repo_{Method}-{Number}]`
5. Hàm ghi (Create/Update) cần gọi `input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);` trước khi lưu.
6. Tham số luôn dùng CancellationToken cho các async methods.

---

## Template: Repository

**File:** `{ServiceName}.Infrastructure/Repositories/{EntityName}Repository.cs`

```csharp
using AutoMapper;
using CommonLibs.DTOs;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Services;
using CommonLibs.Utils;
using Dapper;
using {ServiceName}.Application.Interfaces;
using {ServiceName}.Domain.DTOs;
using {ServiceName}.Domain.Entities;
using {ServiceName}.Infrastructure.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;
using System.Text;

namespace {ServiceName}.Infrastructure.Repositories
{
    public class {EntityName}Repository : BaseRepository<{EntityName}Repository>, I{EntityName}Repository
    {
        private readonly {ServiceName}DbContext _dbContext;
        private readonly string _errorPrefix = "{EntityName}_Repo_";

        public {EntityName}Repository(
            IMapper mapper,
            ILogger<{EntityName}Repository> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContext,
            {ServiceName}DbContext dbContext,
            IOptions<AppOptions> options,
            IAuditContext auditContext,
            IJwtTokenService jwtTokenService)
            : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Repository Methods

        public async Task<{EntityName}OutputDTO> Create{EntityName}({EntityName}InputDTO input, CancellationToken cancellationToken)
        {
            try
            {
                if (input == null)
                    throw new AppException($"[{_errorPrefix}Create{EntityName}-01] Input is null.", HttpStatusCode.BadRequest);

                if (string.IsNullOrWhiteSpace(input.{EntityName}Id))
                    throw new AppException($"[{_errorPrefix}Create{EntityName}-02] {EntityName}Id is required.", HttpStatusCode.BadRequest);

                // Kiểm tra trùng lặp
                if (await Check{EntityName}Exists(input.{EntityName}Id, cancellationToken))
                    throw new AppException($"[{_errorPrefix}Create{EntityName}-03] {EntityName} with Id '{input.{EntityName}Id}' already exists.", HttpStatusCode.Conflict);

                input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

                int rowsAffected = await _dbContext.ExecuteAsync(@"
                    INSERT INTO tbl_{EntityName}s ({EntityName}Id, {EntityName}Name, CreatedDate, CreatedBy, DeviceName, DeviceId)
                    VALUES (@{EntityName}Id, @{EntityName}Name, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
                ", new
                {
                    {EntityName}Id = input.{EntityName}Id,
                    {EntityName}Name = input.{EntityName}Name,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                }, cancellationToken);

                if (rowsAffected <= 0)
                    throw new AppException($"[{_errorPrefix}Create{EntityName}-04] Failed to create {EntityName}.", HttpStatusCode.InternalServerError);

                return _mapper.Map<{EntityName}OutputDTO>(input);
            }
            catch
            {
                throw;
            }
        }

        public async Task<{EntityName}OutputDTO> Get{EntityName}ById(string {entityName}Id, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.QueryFirstOrDefaultAsync<{EntityName}>(
                    @"SELECT * FROM tbl_{EntityName}s WHERE {EntityName}Id = @{EntityName}Id",
                    new { {EntityName}Id = {entityName}Id },
                    cancellationToken);

                if (entity == null)
                    throw new AppException($"[{_errorPrefix}Get{EntityName}ById-01] {EntityName} with Id '{{{entityName}Id}}' not found.", HttpStatusCode.NotFound);

                return _mapper.Map<{EntityName}OutputDTO>(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<BaseFilterOutput<{EntityName}OutputDTO>> GetAll{EntityName}s({EntityName}FilterDTO filter, CancellationToken cancellationToken)
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
                
                if (!string.IsNullOrWhiteSpace(filter.{EntityName}Name))
                {
                    whereBuilder.Append(" AND {EntityName}Name LIKE @{EntityName}Name ");
                    parameters.Add("{EntityName}Name", $"%{filter.{EntityName}Name.Trim()}%", DbType.String);
                }

                // Lọc theo CreatedDate
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

                // Đếm tổng số bản ghi
                string countSql = $"SELECT COUNT(1) FROM tbl_{EntityName}s {whereBuilder}";
                var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

                var filterOutput = new BaseFilterOutput<{EntityName}OutputDTO>
                {
                    TotalRecords = totalCount,
                    TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = new List<{EntityName}OutputDTO>()
                };

                if (totalCount == 0)
                    return filterOutput;

                // Sắp xếp
                string orderByClause = filter.SortBy switch
                {
                    Sort.AutoIdDes => " ORDER BY AutoId DESC ",
                    _ => " ORDER BY AutoId ASC "
                };

                // Phân trang
                string querySql = $"SELECT * FROM tbl_{EntityName}s {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
                parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

                var entities = await _dbContext.QueryAsync<{EntityName}>(querySql, parameters, cancellationToken);

                filterOutput.Records = entities.Select(e => _mapper.Map<{EntityName}OutputDTO>(e)).ToList();

                return filterOutput;
            }
            catch
            {
                throw;
            }
        }

        // Thêm các hàm Update, Delete tùy nghiệp vụ
        #endregion

        #region Helpers

        private async Task<bool> Check{EntityName}Exists(string {entityName}Id, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.QueryFirstOrDefaultAsync<{EntityName}>(
                    @"SELECT * FROM tbl_{EntityName}s WHERE {EntityName}Id = @{EntityName}Id",
                    new { {EntityName}Id = {entityName}Id },
                    cancellationToken);

                return entity != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi kiểm tra {EntityName}Exists");
                throw;
            }
        }

        #endregion
    }
}
```

---

## Cập nhật DI

**File:** `{ServiceName}.Infrastructure/DependencyInjection.cs`

Mở file và thêm dòng sau vào hàm `Add{ServiceName}Infrastructure`:

```csharp
// Đăng ký {EntityName} Repository
services.AddScoped<I{EntityName}Repository, {EntityName}Repository>();
```
