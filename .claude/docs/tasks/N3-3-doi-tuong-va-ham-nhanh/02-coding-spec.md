# N3-3 — ConfigSQL: xem đối tượng, sinh script, hàm hệ thống gọi nhanh · Coding Spec (bản phác thảo)

- **Trạng thái duyệt:** ⬜ chờ duyệt — **bản phác thảo**, BA bổ sung chi tiết (mục 2b, tên hàm, AC đầy đủ) và xin duyệt lại trước khi `/task-run`
- **Yêu cầu gốc:** `../N3-configsql-quan-tri-sql-server/01-requirement.md` mục 4 (bước 5), R4, R6, R7 · **Phụ thuộc: N3-1** (Session, ghi log); nên sau N3-2

## 1. Phạm vi
Tab **Đối tượng** (nhóm 2), **chỉ đọc và sinh script**: liệt kê theo DB đang kết nối các **user/login**, **stored procedure**, **function**, **bảng**, **khóa** (PK/FK/UNIQUE), **view**, **trigger**; chọn một đối tượng để xem định nghĩa (`sys.sql_modules`, cột, khóa, index) và **sinh script** `CREATE`/`ALTER`/`DROP` vào ô script (chỉ đọc/sao chép, **không có nút chạy**). Nhóm **Hàm hệ thống gọi nhanh**: danh sách định sẵn (ví dụ `@@VERSION`, `DB_NAME()`, `sp_helpdb`, `sp_who2`, `sys.dm_*` đọc nhanh) chạy bằng một cú bấm, kết quả vào lưới.
- Mọi truy vấn dùng `QUOTENAME`/tham số hóa (R7), qua `SqlConnectionSession.QueryTableAsync`, timeout 30 giây (R9).
- `SqlScriptGenerator` (mới) là bộ sinh script dùng chung cho N3-4 và N3-5.

## 2. Ngoài phạm vi
- Tạo/sửa/xóa đối tượng trực tiếp bằng form (người dùng tự bấm chạy ở nơi khác), ô soạn SQL tự do, SMO.

## 3. File dự kiến tạo/sửa (chốt lại khi hoàn thiện spec)
| Đường dẫn | Tạo/Sửa |
|---|---|
| `SystemManagement/Utils/SqlObjectCatalog.cs` | Tạo (câu truy vấn catalog) |
| `SystemManagement/Utils/SqlScriptGenerator.cs` | Tạo (sinh CREATE/ALTER/DROP từ `sys.*`) |
| `SystemManagement/Utils/SqlQuickFunctions.cs` | Tạo (danh sách hàm gọi nhanh định sẵn, có đánh dấu đọc/ghi) |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQLObjects.cs` + `.Designer.cs` | Tạo (tab) |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQL.cs` | Sửa (gắn tab) |

## 4. Tiêu chí nghiệm thu (tóm tắt, sẽ chi tiết hóa)
- Liệt kê đúng từng loại đối tượng; số lượng khớp `sys.objects`/`sys.database_principals` trên DB thử.
- Sinh script bảng đúng cột, kiểu, null, default, PK/FK/UNIQUE/index; script `DROP` có `IF EXISTS`; tên đối tượng có ký tự đặc biệt được `QUOTENAME` đúng.
- Hàm gọi nhanh chỉ gồm danh sách định sẵn và chỉ chạy lệnh đọc khi đang Chỉ đọc; không có ô nhập lệnh tự do.
- Thiếu quyền (`VIEW DEFINITION`...) báo lỗi cho mục đó, không làm hỏng tab.
- Logic sinh script kiểm bằng harness trên DB thử; runtime cần 🔑 SQL Server.
