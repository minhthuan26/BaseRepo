---
name: create-domain-layer
description: >
  Tạo Entity trong tầng Domain theo Clean Architecture cho BaseRepo project.
  Sử dụng khi cần tạo Entity mới: kế thừa BaseEntity với guard methods, constructor từ DataRow.
  DTOs (Input/Output/Filter) KHÔNG còn tạo ở đây — xem skill `create-contract-layer`.
---

# Skill: Tạo Domain Layer

## Tổng quan

Tầng **Domain** là lõi của kiến trúc, chứa business logic thuần túy (Entity). Tầng này **không phụ thuộc** vào bất kỳ tầng nào khác ngoài `CommonLibs`.

> **Lưu ý (2026-09-01):** DTO (Input/Output/Filter) đã tách khỏi Domain sang project `{ServiceName}.Contract`
> cho cả 3 service hiện có (Gateway, Site, AdministrativeUnit). Xem skill `create-contract-layer` để tạo DTO.

### Thành phần cần tạo

| # | File | Mô tả |
|---|---|---|
| 1 | `{ServiceName}.Domain/Entities/{EntityName}.cs` | Rich Domain Entity |

DTO (Input, Output, Filter): xem skill `create-contract-layer`.

---

## Quy tắc

1. **Entity** kế thừa `BaseEntity` từ `CommonLibs.Entities`
2. Entity sử dụng **private setter** + public **guard methods** (Set methods) để bảo vệ invariant
3. Entity phải có **constructor từ DataRow** để hỗ trợ đọc dữ liệu từ Dapper
4. Entity phải có **protected parameterless constructor** cho EF Core

---

## BaseEntity (từ CommonLibs) — Tham khảo

```csharp
// CommonLibs/Entities/BaseEntity.cs
public class BaseEntity
{
    public long AutoId { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    public string? DeviceName { get; set; }
    public string? DeviceId { get; set; }
}

public enum EntityStatus
{
    Active = 1,
    Inactive = 0,
    Deleted = -1
}
```

## Base DTOs (từ CommonLibs) — Tham khảo

```csharp
// BaseOutputDTO: AutoId, UpdatedDate, CreatedDate, CreatedBy, UpdatedBy, Status (string), DeviceName, DeviceId
// BaseInputDTO:  UpdatedDate, CreatedDate, CreatedBy, UpdatedBy, Status (EntityStatus?), DeviceName, DeviceId
//                + HeaderMappings (virtual) + SetAuditInfo()
// BaseFilterDTO: Status (int?), PageNumber, PageSize, FromDate, ToDate, SortBy
// BaseFilterOutput<T>: TotalRecords, TotalPages, PageNumber, PageSize, Records (List<T>)
```

---

## Template: Entity

**File:** `{ServiceName}.Domain/Entities/{EntityName}.cs`

```csharp
using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace {ServiceName}.Domain.Entities
{
    public class {EntityName} : BaseEntity
    {
        // --- Properties (private setter) ---
        public string {EntityName}Id { get; private set; } = null!;
        public string {EntityName}Name { get; private set; } = null!;
        // Thêm các property khác tùy nghiệp vụ:
        // public string? Description { get; private set; }
        // public int Priority { get; private set; } = 1;
        // public string? Email { get; private set; }

        // --- Constructors ---

        /// <summary>
        /// EF Core cần constructor không tham số
        /// </summary>
        protected {EntityName}() { }

        /// <summary>
        /// Constructor với các field bắt buộc
        /// </summary>
        public {EntityName}(string {entityName}Id, string {entityName}Name)
        {
            Set{EntityName}Id({entityName}Id);
            Set{EntityName}Name({entityName}Name);
        }

        /// <summary>
        /// Constructor từ DataRow — dùng khi đọc dữ liệu từ SQL Server qua Dapper
        /// </summary>
        public {EntityName}(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));

            Set{EntityName}Id(
                row.Field<string>("{EntityName}Id")
                ?? throw new AppException("{EntityName}Id is required"));

            Set{EntityName}Name(
                row.Field<string>("{EntityName}Name")
                ?? throw new AppException("{EntityName}Name is required"));

            // Thêm set các field khác từ DataRow:
            // SetDescription(row.Field<string?>("Description"));
            // SetPriority(row.Field<int?>("Priority") ?? 1);
        }

        // --- Guard Methods / Setters ---

        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");
            AutoId = autoId;
        }

        public void Set{EntityName}Id(string {entityName}Id)
        {
            if (string.IsNullOrWhiteSpace({entityName}Id))
                throw new AppException("{EntityName}Id could not be empty");
            {EntityName}Id = {entityName}Id.Trim();
        }

        public void Set{EntityName}Name(string {entityName}Name)
        {
            if (string.IsNullOrWhiteSpace({entityName}Name))
                throw new AppException("{EntityName}Name could not be empty");
            {EntityName}Name = {entityName}Name.Trim();
        }

        // Thêm các setter khác với validation logic:
        // public void SetEmail(string? email)
        // {
        //     if (!string.IsNullOrWhiteSpace(email) && !email.Contains("@"))
        //         throw new AppException("Invalid email format");
        //     Email = email;
        // }
    }
}
```

---

## Ví dụ thực tế — Entity Product trong service Site

### File: `Site.Domain/Entities/Product.cs`

```csharp
using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace Site.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string ProductId { get; private set; } = null!;
        public string ProductName { get; private set; } = null!;
        public decimal Price { get; private set; }
        public string? Description { get; private set; }

        protected Product() { }

        public Product(string productId, string productName, decimal price)
        {
            SetProductId(productId);
            SetProductName(productName);
            SetPrice(price);
        }

        public Product(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetProductId(row.Field<string>("ProductId") ?? throw new AppException("ProductId is required"));
            SetProductName(row.Field<string>("ProductName") ?? throw new AppException("ProductName is required"));
            SetPrice(row.Field<decimal>("Price"));
            SetDescription(row.Field<string?>("Description"));
        }

        public void SetAutoId(long autoId)
        {
            if (autoId <= 0) throw new AppException("AutoId must be greater than 0");
            AutoId = autoId;
        }

        public void SetProductId(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId)) throw new AppException("ProductId could not be empty");
            ProductId = productId.Trim();
        }

        public void SetProductName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName)) throw new AppException("ProductName could not be empty");
            ProductName = productName.Trim();
        }

        public void SetPrice(decimal price)
        {
            if (price < 0) throw new AppException("Price must be >= 0");
            Price = price;
        }

        public void SetDescription(string? description) => Description = description;
    }
}
```

DTO tương ứng (`ProductOutputDTO`/`ProductInputDTO`/`ProductFilterDTO`) — xem skill `create-contract-layer`,
đặt tại `Site.Contract/DTOs/ProductDTO.cs`, namespace `Site.Contract.DTOs`.
