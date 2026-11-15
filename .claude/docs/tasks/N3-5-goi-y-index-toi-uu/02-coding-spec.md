# N3-5 — ConfigSQL: gợi ý index và tối ưu query từ DMV · Coding Spec (bản phác thảo)

- **Trạng thái duyệt:** ⬜ chờ duyệt — **bản phác thảo**, BA bổ sung chi tiết và xin duyệt lại trước khi `/task-run`
- **Yêu cầu gốc:** `../N3-configsql-quan-tri-sql-server/01-requirement.md` mục 4 (bước 7), R4, R6, R11 · **Phụ thuộc: N3-1, N3-3** (dùng `SqlScriptGenerator`)

## 1. Phạm vi
Tab **Tối ưu** (nhóm 4), **gợi ý và sinh script, không tự tạo index**:
- Missing index (`sys.dm_db_missing_index_*`) kèm điểm ưu tiên (độ cải thiện ước tính), index **trùng**/**không dùng** (`sys.dm_db_index_usage_stats`), query tốn tài nguyên (`sys.dm_exec_query_stats`: top theo CPU/reads/duration) kèm văn bản query và plan handle.
- Sinh script `CREATE INDEX` (tên theo quy ước `IX_<Bảng>_<Cột>`) hoặc `DROP INDEX` vào ô script để người dùng xem rồi tự chạy ở nơi khác; nêu cảnh báo rằng DMV bị xóa khi SQL Server khởi động lại và gợi ý chỉ là tham khảo.
- DMV rỗng thì hiện "chưa có dữ liệu gợi ý", không coi là lỗi (R11).

## 2. Ngoài phạm vi
- Tự động tạo/xóa index, sửa lại câu query của ứng dụng, phân tích execution plan XML chi tiết, Query Store.

## 3. File dự kiến (chốt lại khi hoàn thiện spec)
`SqlIndexAdvisor.cs`, `UC_ConfigSQLOptimize.cs` + `.Designer.cs`, `UC_ConfigSQL.cs` (gắn tab).

## 4. Tiêu chí nghiệm thu (tóm tắt, sẽ chi tiết hóa)
- Trên DB thử có bảng thiếu index và workload mô phỏng, tab liệt kê đúng gợi ý; script sinh ra chạy được (kiểm tay) và loại bỏ gợi ý khi đã tạo index.
- Server mới khởi động (DMV rỗng) không lỗi.
- Thiếu `VIEW SERVER STATE` báo rõ cho tính năng này, tab khác không hỏng.
- Mọi truy vấn chỉ đọc, qua Session, timeout 30 giây; 🔑 SQL Server.
