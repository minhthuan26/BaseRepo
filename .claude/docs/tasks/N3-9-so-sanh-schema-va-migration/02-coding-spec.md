# N3-9 — ConfigSQL: so sánh schema nguồn → đích và migration tự động (thêm cột, sửa kiểu, tự xử lý phụ thuộc) · Coding Spec (bản phác thảo)

- **Trạng thái duyệt:** ⬜ chờ duyệt — **bản phác thảo**, BA bổ sung chi tiết (tên hàm, câu SQL catalog, danh sách trường hợp không hỗ trợ đã kiểm chứng, AC đầy đủ) và xin duyệt lại trước khi `/task-run`; nhiều khả năng phải **tách** (so sánh | migration cột | tự xử lý phụ thuộc) khi viết chi tiết vì vượt ~8 file
- **Yêu cầu gốc:** `../N3-configsql-quan-tri-sql-server/01-requirement.md` mục 4 (bước 10), R5, R13, R14, R19, R20, R21, R23 · **Phụ thuộc: N3-1** (nhiều kết nối), **N3-2** (`SqlScriptRunner`, `FrmSqlDangerConfirm`, `ViewActionLogger`), **N3-3** (`SqlScriptGenerator`, catalog); dùng `SqlBackupService` của N3-4 nếu đã có để đề nghị backup

## 1. Phạm vi
Tab **So sánh & Migration** (chỉ **cấu trúc**; **không** so sánh dữ liệu từng dòng): chọn **một nguồn** và **một hoặc nhiều đích** (kết nối Sẵn sàng, đích phải bật Cho phép ghi khi thực thi, không trùng nguồn).
- **So sánh schema** nguồn → từng đích: bảng thiếu ở đích; cột thiếu; cột thừa ở đích; cột khác **kiểu, độ dài, precision/scale, NULL**; khác default/collation/identity/computed (chỉ báo cáo). Khớp bảng theo `schema.tên`, cột theo tên, không phân biệt hoa thường. Lưới kết quả theo cây Bảng → Cột, tô màu theo loại khác biệt và mức rủi ro; lọc theo loại; **Xuất CSV**.
- **Sinh kế hoạch migration theo bảng nguồn** (xem trước toàn bộ script, từng thay đổi tick chọn riêng):
  - tạo bảng thiếu (dùng `SqlScriptGenerator`);
  - `ALTER TABLE ... ADD` cột thiếu (cột NOT NULL không default vào bảng có dữ liệu thì sinh cột `NULL` kèm cảnh báo);
  - `ALTER TABLE ... ALTER COLUMN` sửa kiểu/độ dài/precision/NULL theo nguồn.
  - **Không bao giờ** sinh lệnh xóa cột hoặc bảng thừa ở đích.
- **Tự xử lý phụ thuộc (R23), không để người dùng xử lý thủ công:** khi cột cần sửa nằm trong default/CHECK/index/PK/UNIQUE/FK/thống kê, kế hoạch tự sinh: drop FK liên quan → drop default/CHECK → drop index/PK/UNIQUE/thống kê chứa cột → `ALTER COLUMN` → tạo lại **đúng định nghĩa cũ** (tên, cột key/include, thứ tự, UNIQUE, clustered/nonclustered, filter, fill factor, `IGNORE_DUP_KEY`, FK với `ON DELETE/UPDATE`, trạng thái enabled/not trusted). Các cột nối nhau qua FK xử lý **theo nhóm** (drop mọi FK của nhóm, sửa mọi cột, tạo lại FK); hai đầu FK vẫn khác kiểu sau khi sửa thì chặn kèm lý do.
- **Mức rủi ro** từng thay đổi: *An toàn* (mở rộng độ dài, `int` → `bigint`...), *Cần kiểm tra dữ liệu* (thu hẹp, đổi kiểu, chuyển sang NOT NULL), *Cần tự xử lý phụ thuộc* (có dependency như trên, xử lý tự động), *Hành động nặng* (drop/tạo lại clustered index hoặc PK trên bảng lớn, tạo lại FK `WITH CHECK`: hiện ước lượng số dòng và cảnh báo khóa/log), *Không hỗ trợ tự động* (xem dưới).
- **Không hỗ trợ tự động** (báo rõ lý do từng cột, loại khỏi script, không bỏ qua im lặng): view/function `SCHEMABINDING` tham chiếu cột, cột computed hoặc cột mà computed phụ thuộc, full-text index, bảng memory-optimized, bảng temporal (system-versioned), bảng bật CDC/replication, cột identity nếu SQL Server không cho đổi, cột Always Encrypted/masking/ledger, kiểu CLR hoặc xml schema collection. **Danh sách này sẽ được chốt và kiểm chứng trên SQL Server thật khi viết spec chi tiết**; mục tiêu là thu hẹp tối đa phần phải xử lý ngoài phần mềm.
- **Kiểm tra tương thích trước khi sửa kiểu** (chỉ đọc, chạy trên đích): ví dụ `MAX(LEN(col))` vượt độ dài mới, số dòng `TRY_CONVERT` thất bại, số dòng NULL khi chuyển NOT NULL. Có vi phạm thì **chặn** thay đổi đó và liệt kê số dòng.
- **Thực thi trực tiếp** trên các đích đã chọn theo R20: tuần tự, **mỗi đích một transaction duy nhất** gồm cả drop, sửa và tạo lại (DDL của SQL Server rollback được; lỗi bước nào thì rollback nguyên vẹn đích đó), tùy chọn *Dừng khi một đích lỗi* (mặc định bật), xác nhận mạnh gộp một hộp (1 đích gõ tên DB, nhiều đích gõ số lượng đích), đề nghị backup `COPY_ONLY` trước, ghi nhật ký `Logs/ConfigSQL/` một dòng mỗi đích.

## 2. Ngoài phạm vi
- **So sánh dữ liệu từng dòng và đồng bộ dữ liệu** (người dùng đã loại; copy dữ liệu do N3-4 lo), tự xóa cột/bảng thừa, đổi identity, so sánh/migration view-SP-function-trigger (có thể làm vòng sau), chạy song song nhiều đích, distributed transaction.

## 3. File dự kiến (chốt lại khi hoàn thiện spec, có thể tách task)
| Đường dẫn | Tạo/Sửa |
|---|---|
| `SystemManagement/Utils/SqlSchemaComparer.cs` | Tạo (đọc catalog hai phía, so khớp, phân loại khác biệt và rủi ro) |
| `SystemManagement/Utils/SqlMigrationPlanner.cs` | Tạo (sinh script ADD/ALTER, câu kiểm tra tương thích, sắp thứ tự, nhóm FK) |
| `SystemManagement/Utils/SqlDependencyScripter.cs` | Tạo (đọc phụ thuộc của cột, sinh script drop và tạo lại index/PK/UNIQUE/FK/CHECK/default/thống kê đúng định nghĩa cũ, phát hiện trường hợp không hỗ trợ) |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQLSchemaCompare.cs` + `.Designer.cs` | Tạo (tab: chọn nguồn/đích, lưới khác biệt, xem trước script, chạy) |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQL.cs` | Sửa (gắn tab) |

## 4. Tiêu chí nghiệm thu (tóm tắt, sẽ chi tiết hóa)
- 2 DB thử có chủ ý lệch: đích thiếu một bảng, thiếu một cột, một cột `varchar(50)` so với `varchar(100)` ở nguồn, một cột `NULL` so với `NOT NULL`, một cột thừa. So sánh liệt kê đúng từng khác biệt và loại; cột thừa chỉ báo cáo.
- Sau migration, so sánh lại cho kết quả **0 khác biệt** (trừ nhóm chỉ báo cáo và nhóm không hỗ trợ tự động); script chạy đúng **trên từng đích**.
- **Tự xử lý phụ thuộc:** đổi kiểu một cột đang nằm trong PK clustered, một index nonclustered có INCLUDE và filter, một default, một CHECK, một FK (cả hai đầu) và một thống kê người dùng: migration chạy thành công, **mọi đối tượng được tạo lại giống hệt** (so `sys.indexes`, `sys.foreign_keys`, `sys.default_constraints`, `sys.check_constraints` trước/sau), dữ liệu còn nguyên.
- Một bước tạo lại bị lỗi (ví dụ FK `WITH CHECK` gặp dữ liệu vi phạm): **rollback toàn bộ đích đó**, schema trở về như trước, không còn đối tượng bị drop dở.
- Thu hẹp kiểu khi đích có dữ liệu vượt giới hạn: bị **chặn**, báo đúng số dòng vi phạm.
- Mỗi trường hợp **không hỗ trợ tự động** hiện đúng lý do và không có trong script; các thay đổi còn lại vẫn chạy.
- Thêm cột NOT NULL không default vào bảng có dữ liệu: sinh `NULL` kèm cảnh báo. Bảng lớn drop/tạo lại clustered index: hiện cảnh báo và xác nhận mạnh.
- 1 nguồn + 3 đích: một đích lỗi thì rollback đích đó, đích đã xong giữ nguyên, đích chưa chạy theo tùy chọn Dừng/Tiếp tục; đích trùng nguồn bị chặn.
- Chế độ Chỉ đọc khóa nút Chạy (xem trước script vẫn dùng được); nhật ký không chứa dữ liệu hay password. Logic so khớp và sinh script kiểm bằng harness; chạy thật cần 🔑 SQL Server, DB thử.
