# Tách file SQL về từng service + màn hình "SQL" trong SystemManagement — Thiết kế

> Spec này mô tả **toàn bộ thiết kế đã chốt** (cả phần di chuyển file lẫn màn hình SystemManagement),
> nhưng đợt triển khai đầu tiên (xem plan đi kèm) **chỉ làm phần di chuyển file SQL** — phần màn hình
> `UC_ConfigSQL` trong SystemManagement để người dùng tự làm sau, spec này là tài liệu tham chiếu cho
> đợt đó.

## 1. Bối cảnh & Mục tiêu

Hiện tại mọi file `.sql` cài đặt/seed dữ liệu nằm tập trung tại `CommonLibs/BaseSQL/` — không gắn với
service nào cụ thể dù mỗi file chỉ phục vụ đúng 1 service (`BaseSQL.sql`/`seed_admin_account.sql` cho
Gateway, `BaseAdministrativeUnit.sql`/`seed_administrative_unit.sql` cho AdministrativeUnit,
`BaseBooking.sql` cho Booking). Mục tiêu: di chuyển mỗi file `.sql` (và tài liệu đi kèm) vào đúng thư
mục `{Service}.Api/SQL/` của service sở hữu nó, để sau này công cụ vận hành `SystemManagement` có thể
đọc SQL trực tiếp từ đúng thư mục triển khai vật lý của từng service (`ModuleItem.FolderPath` đã có sẵn
trong model, xem mục 4) mà không cần biết gì về cấu trúc `CommonLibs/BaseSQL/` cũ.

## 2. Phạm vi

### Đợt này (có plan triển khai đi kèm)
- Di chuyển toàn bộ file `.sql`/`README.md` liên quan sang `{Service}.Api/SQL/` tương ứng.
- Cập nhật `.csproj` của 3 `.Api` project để file trong `SQL/` được copy ra output khi build/publish.
- Cập nhật mọi tài liệu (`claude.md`, `.claude/rules/`, README còn lại) trỏ đúng vị trí file mới.

### Đợt sau (người dùng tự làm, KHÔNG nằm trong plan đợt này)
- Màn hình `UC_ConfigSQL` trong SystemManagement (mục 5-6 dưới đây) — chỉ ghi lại thiết kế đã chốt để
  tham chiếu khi triển khai.

## 3. Cấu trúc file đích

```
Base/Gateway/Gateway.Api/SQL/
├── BaseSQL.sql
└── seed_admin_account.sql

Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/
├── BaseAdministrativeUnit.sql
├── seed_administrative_unit.sql
└── README.md

SportsBooking/Booking/Booking.Api/SQL/
└── BaseBooking.sql
```

**Không di chuyển**: `CommonLibs/BaseSQL/multi-tenant/` (README.md + `tests/*.sql`/`*.sh`) — đây là tài
liệu thiết kế + bộ test cách ly 2 Site, không phải file cài đặt/seed, ngoài phạm vi tính năng này (đã
chốt qua trao đổi trực tiếp). `Site.Api` không có thư mục `SQL/` riêng — Site dùng chung đúng
`BaseSQL.sql` với Gateway (cùng database `BaseRepo`), file này đã được gán cho Gateway sở hữu.

## 4. Chuẩn bị hạ tầng cho `SystemManagement` (đợt sau dùng, đợt này KHÔNG động vào code SystemManagement)

`SystemManagement/Models/SystemConfig.cs` đã có sẵn đúng cơ chế cần thiết — `SiteItem` chứa danh sách
`ModuleItem`, mỗi `ModuleItem` có `FolderPath` (đường dẫn vật lý nơi service được triển khai). Đây chính
là chỗ `UC_ConfigSQL` (đợt sau) sẽ đọc để tìm `{FolderPath}\SQL\*.sql`. Việc file `.sql` đã ở đúng
`{Service}.Api/SQL/` (mục 3) + được copy ra output khi publish (mục 2) là điều kiện tiên quyết bắt buộc
để `FolderPath` (trỏ vào thư mục publish) tìm thấy đúng file.

## 5. Data model bổ sung (đợt sau)

```csharp
public class ModuleItem
{
    // ... các field hiện có (Name, FolderPath, ConfigurationPath, ConfigurationFileType, IisName, IisPort)
    public string DefaultConnection { get; set; }  // MỚI — tên Connection gợi ý sẵn khi chọn Module này
}
```

`system.config` mẫu bổ sung thêm 3 `<module>` (`Site`, `AdministrativeUnit`, `Booking`) cạnh `Gateway`
hiện có, `folderPath` để trống cho người dùng tự điền theo máy triển khai thật.

## 6. Màn hình `UC_ConfigSQL` (đợt sau)

Theo đúng khuôn `UC_ConfigEnv` đã có (implement `IBasePage`, gọi `BaseUC.InternalNavigate()`):

- **Chọn Module** (dropdown, gộp mọi Site trong `SysConfig.Sites`) → tự gợi ý **Connection** theo
  `ModuleItem.DefaultConnection` (đổi được qua dropdown `SysConfig.Connections`).
- **Danh sách file** `*.sql` quét từ `{Module.FolderPath}\SQL\` (không đệ quy).
- Chọn 1 file → **preview nội dung** (read-only, TextBox/RichTextBox).
- Nút **Chạy** → dialog xác nhận (tên file + Host/Database đích) → xác nhận mới thực thi thật (không
  chạy thẳng khi bấm nút — đã chốt vì nhiều file có `CREATE TABLE`/dữ liệu, chạy nhầm/chạy 2 lần có thể
  lỗi hoặc mất dữ liệu).
- **Bộ thực thi** (`SystemManagement/Utils/SqlScriptRunner.cs`, mới) — tự tách file theo dòng `GO` riêng
  biệt (không phụ thuộc `sqlcmd.exe`, không yêu cầu cài thêm gì trên máy chạy SystemManagement), chạy
  tuần tự từng batch qua `SqlConnection`/`SqlCommand` (connection string dựng từ `ConnectionItem` đã
  chọn). **Dừng ngay khi 1 batch lỗi** — không chạy tiếp các batch sau, hiện rõ batch nào lỗi + nội dung
  lỗi trong panel kết quả.
- **Chỉ chạy 1 file/lần** (đã chốt, không hỗ trợ chọn nhiều file chạy tuần tự trong 1 lần bấm) — người
  dùng tự chạy tuần tự từng file, xem kết quả từng bước trước khi qua file tiếp theo.
- `FrmMain` thêm 1 mục menu "SQL" (theo đúng pattern `LoadPage`/`GoBack` hiện có) điều hướng vào
  `UC_ConfigSQL`.

## 7. Ngoài phạm vi (mọi đợt)

- Mã hoá `ConnectionItem.Password` trong `system.config` — hiện đang lưu dạng chuỗi thường trong XML,
  giữ nguyên convention hiện có của model này (không phải phát sinh mới từ tính năng này).
- Cơ chế tracking "đã chạy file nào rồi" (migration history table) — không nằm trong yêu cầu, người
  dùng tự chịu trách nhiệm không chạy trùng.
