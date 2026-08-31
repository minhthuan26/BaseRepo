---
name: create-contract-layer
description: >
  Tạo DTOs (Input, Output, Filter) theo Clean Architecture cho BaseRepo project.
  Sử dụng khi cần tạo OutputDTO kế thừa BaseOutputDTO, InputDTO kế thừa BaseInputDTO, FilterDTO kế thừa BaseFilterDTO
  cho một entity mới, trong bất kỳ service nào (Gateway, Site, AdministrativeUnit, hoặc service mới).
---

# Skill: Tạo Contract Layer (DTOs)

## Tổng quan

DTO (Input/Output/Filter) sống ở project riêng `{ServiceName}.Contract` (Contract layer chỉ phụ thuộc
`CommonLibs`, không phụ thuộc Domain/Application/Infrastructure). Cả 3 service hiện có (Gateway, Site,
AdministrativeUnit) đều đã tách theo khuôn này — khi tạo service mới, tạo `{ServiceName}.Contract` ngay từ đầu
thay vì đặt DTO trong Domain.

### Thành phần cần tạo

| # | File | Mô tả |
|---|---|---|
| 1 | `{ServiceName}.Contract/DTOs/{EntityName}DTO.cs` | Input, Output, Filter DTOs |

---

## Quy tắc

1. **OutputDTO** kế thừa `BaseOutputDTO` từ `CommonLibs.DTOs`
2. **InputDTO** kế thừa `BaseInputDTO` từ `CommonLibs.DTOs`
3. **FilterDTO** kế thừa `BaseFilterDTO` từ `CommonLibs.DTOs`
4. InputDTO cần override `HeaderMappings` nếu cần chức năng Import Excel/CSV
5. Contract/DTO layer **không** được reference Entity (Domain) — DTO là hình dạng dữ liệu qua lại API, không phải
   domain model; chuyển đổi Entity ↔ DTO làm ở AutoMapper (`{ServiceName}.Application/Mappings/`).

---

## Base DTOs (từ CommonLibs) — Tham khảo

```csharp
// BaseOutputDTO: AutoId, UpdatedDate, CreatedDate, CreatedBy, UpdatedBy, Status (string), DeviceName, DeviceId
// BaseInputDTO:  UpdatedDate, CreatedDate, CreatedBy, UpdatedBy, Status (EntityStatus?), DeviceName, DeviceId
//                + HeaderMappings (virtual) + SetAuditInfo()
// BaseFilterDTO: Status (int?), PageNumber, PageSize, FromDate, ToDate, SortBy
// BaseFilterOutput<T>: TotalRecords, TotalPages, PageNumber, PageSize, Records (List<T>)
```

---

## Template: DTOs

**File:** `{ServiceName}.Contract/DTOs/{EntityName}DTO.cs`

```csharp
using CommonLibs.DTOs;

namespace {ServiceName}.Contract.DTOs
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

## Ví dụ thực tế

### Gateway — `Gateway.Contract/DTOs/UserDTO.cs`

```csharp
using CommonLibs.DTOs;

namespace Gateway.Contract.DTOs
{
    public class UserOutputDTO : BaseOutputDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        // ...
    }
}
```

### Site — `Site.Contract/DTOs/ProductDTO.cs`

```csharp
using CommonLibs.DTOs;

namespace Site.Contract.DTOs
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
