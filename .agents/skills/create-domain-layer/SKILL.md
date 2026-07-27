---
name: create-domain-layer
description: >
  Tạo Entity và DTOs (Input, Output, Filter) trong tầng Domain theo Clean Architecture cho BaseRepo project.
  Sử dụng khi cần tạo các thành phần Domain Layer cho một entity mới: Entity kế thừa BaseEntity với guard methods,
  OutputDTO kế thừa BaseOutputDTO, InputDTO kế thừa BaseInputDTO, FilterDTO kế thừa BaseFilterDTO.
---

# Skill: Tạo Domain Layer

## Tổng quan

Tầng **Domain** là lõi của kiến trúc, chứa business logic thuần túy. Tầng này **không phụ thuộc** vào bất kỳ tầng nào khác ngoài `CommonLibs`.

### Thành phần cần tạo

| # | File | Mô tả |
|---|---|---|
| 1 | `{ServiceName}.Domain/Entities/{EntityName}.cs` | Rich Domain Entity |
| 2 | `{ServiceName}.Domain/DTOs/{EntityName}DTO.cs` | Input, Output, Filter DTOs |

---

## Quy tắc

1. **Entity** kế thừa `BaseEntity` từ `CommonLibs.Entities`
2. **OutputDTO** kế thừa `BaseOutputDTO` từ `CommonLibs.DTOs`
3. **InputDTO** kế thừa `BaseInputDTO` từ `CommonLibs.DTOs`
4. **FilterDTO** kế thừa `BaseFilterDTO` từ `CommonLibs.DTOs`
5. Entity sử dụng **private setter** + public **guard methods** (Set methods) để bảo vệ invariant
6. Entity phải có **constructor từ DataRow** để hỗ trợ đọc dữ liệu từ Dapper
7. Entity phải có **protected parameterless constructor** cho EF Core
8. InputDTO cần override `HeaderMappings` nếu cần chức năng Import Excel/CSV

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

## Template: DTOs

**File:** `{ServiceName}.Domain/DTOs/{EntityName}DTO.cs`

```csharp
using CommonLibs.DTOs;

namespace {ServiceName}.Domain.DTOs
{
    /// <summary>
    /// DTO trả về cho client
    /// </summary>
    public class {EntityName}OutputDTO : BaseOutputDTO
    {
        public string {EntityName}Id { get; set; }
        public string {EntityName}Name { get; set; }
        // Thêm các property output tương ứng với Entity
        // Lưu ý: Có thể khác Entity (VD: không trả về Password, byte[] chuyển thành string...)
    }

    /// <summary>
    /// DTO nhận từ client khi tạo/cập nhật
    /// </summary>
    public class {EntityName}InputDTO : BaseInputDTO
    {
        public string {EntityName}Id { get; set; }
        public string {EntityName}Name { get; set; }
        // Thêm các property input

        /// <summary>
        /// Mapping header cho chức năng Import từ Excel/CSV.
        /// Key = Tên cột trong file Excel, Value = Tên property trong DTO
        /// </summary>
        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["{EntityName}Id"] = nameof({EntityName}Id),
            ["{EntityName}Name"] = nameof({EntityName}Name),
            ["Status"] = nameof(Status)
            // Thêm mapping cho các field khác
        };
    }

    /// <summary>
    /// DTO filter để lọc danh sách, kế thừa sẵn: Status, PageNumber, PageSize, FromDate, ToDate, SortBy
    /// </summary>
    public class {EntityName}FilterDTO : BaseFilterDTO
    {
        public string? {EntityName}Name { get; set; }
        // Thêm các filter field riêng
        // public string? Email { get; set; }
        // public int? Priority { get; set; }
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

### File: `Site.Domain/DTOs/ProductDTO.cs`

```csharp
using CommonLibs.DTOs;

namespace Site.Domain.DTOs
{
    public class ProductOutputDTO : BaseOutputDTO
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }

    public class ProductInputDTO : BaseInputDTO
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["ProductId"] = nameof(ProductId),
            ["ProductName"] = nameof(ProductName),
            ["Price"] = nameof(Price),
            ["Status"] = nameof(Status)
        };
    }

    public class ProductFilterDTO : BaseFilterDTO
    {
        public string? ProductName { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
```
