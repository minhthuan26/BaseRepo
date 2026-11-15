---
description: "Convention SQL (SQL Server, truy cập: Dapper qua BaseDbContext)"
paths:
  - '**/*.sql'
---

# Convention SQL (SQL Server, truy cập: Dapper qua BaseDbContext)

> Rút ra từ script thật trong `{Service}.Api/SQL/` (VD `Base/Gateway/Gateway.Api/SQL/BaseSQL.sql`,
> `SportsBooking/Booking/Booking.Api/SQL/BaseBooking.sql`).

## Đặt tên
| Đối tượng | Quy tắc | Ví dụ |
|---|---|---|
| Bảng (service nền tảng Base cũ) | `tbl_` + PascalCase, giữ nguyên kiểu cũ | `tbl_Users`, `tbl_Role_Function` |
| Bảng (service mới) | PascalCase, số ít, không tiền tố | `Field`, `PricingRule`, `Province` |
| Cột | PascalCase | `SiteId`, `CreatedDate` |
| Khóa chính | `AutoId BIGINT IDENTITY(1,1)` PRIMARY KEY CLUSTERED (mọi bảng) | `AutoId` |
| Khóa nghiệp vụ | cột riêng + `UNIQUE`, tên `UQ_<Bảng>_<Cột>` | `UQ_tbl_Roles_SiteId_RoleId` |
| Index | `IX_<Bảng>_<Cột>` | `IX_tbl_Role_Function_FunctionId_RoleId` |
| Stored procedure | hiện **chưa có SP nào**; nếu thêm: `usp_<Đối tượng>_<Hành động>`, gọi qua `ExecuteProcAsync()` | `usp_Booking_CheckOverlap` |

- Mọi bảng có đủ cột audit khớp `BaseEntity`: `Status`, `CreatedDate`, `CreatedBy`, `UpdatedDate`, `UpdatedBy`, `DeviceId`, `DeviceName`.
- Xoá mềm bằng `Status = -1`; truy vấn đọc lọc `Status = 1`.

## Script và migration
- Script đặt trong `{Service}.Api/SQL/` của service sở hữu (đã khai `<Content Include="SQL\**" ...>` trong `.csproj`).
- Mỗi thay đổi schema là **một script mới**; không sửa script đã chạy lên DB.
- Script chạy lại được an toàn khi có thể (`IF NOT EXISTS`, `IF OBJECT_ID(...) IS NULL`).
- Mỗi service dùng database riêng của nó (`USE <DbName>` đầu script).

## Truy vấn (Dapper trong Repository)
- Luôn tham số hóa (`@Param`), không nối chuỗi từ input.
- Phân trang bằng `OFFSET/FETCH` kèm `ORDER BY` xác định.
- Thao tác nhiều bước dùng `ExecuteWithTransactionAsync()`; bulk dùng `ExecuteBulkAsync()`.
- Tránh hàm trên cột trong `WHERE` làm mất index; lọc theo `SiteId` cho dữ liệu Site-scoped.
