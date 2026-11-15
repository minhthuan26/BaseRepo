# N3-8 — ConfigSQL: thư viện script chẩn đoán (CPU, IO, wait, memory, log hệ thống...) · Coding Spec (bản phác thảo)

- **Trạng thái duyệt:** ⬜ chờ duyệt — **bản phác thảo**, BA bổ sung chi tiết (tên hàm, câu SQL, AC đầy đủ) và xin duyệt lại trước khi `/task-run`
- **Yêu cầu gốc:** `../N3-configsql-quan-tri-sql-server/01-requirement.md` mục 4 (bước 9), R4, R6, R7, R9, R11, R18 · **Phụ thuộc: N3-1** (Session, ghi log); độc lập với N3-3..N3-7

## 1. Phạm vi
Tab **Thư viện script** (chỉ đọc): danh mục script chẩn đoán dựng sẵn, chọn một script → (nhập tham số nếu có) → chạy một cú bấm → kết quả vào lưới, có nút **Xuất CSV**. Script nằm cố định trong code, **người dùng không sửa được và không có ô nhập SQL tự do**. Chạy được cả khi đang ở chế độ Chỉ đọc.

Mỗi script có: tên, nhóm, mô tả ngắn, tham số (nếu có), quyền/phiên bản cần, cờ **nặng** (cần xác nhận Yes/No trước khi chạy, timeout dài hơn). Danh mục ban đầu:

| Nhóm | Script (nguồn) |
|---|---|
| **CPU** | Top query theo CPU (`sys.dm_exec_query_stats` + `dm_exec_sql_text`); request đang chạy theo CPU (`dm_exec_requests`); CPU lịch sử gần đây (`dm_os_ring_buffers`, scheduler monitor) |
| **IO** | Top query theo logical/physical reads và writes; **độ trễ IO theo file** (`dm_io_virtual_file_stats`: io_stall đọc/ghi, số lần); IO đang chờ (`dm_io_pending_io_requests`) |
| **Wait** | Wait stats tích lũy (`dm_os_wait_stats`, lọc wait vô hại); task đang chờ (`dm_os_waiting_tasks`) |
| **Memory** | Bộ nhớ hệ thống/tiến trình (`dm_os_sys_memory`, `dm_os_process_memory`); memory grant đang chờ (`dm_exec_query_memory_grants`); buffer pool theo DB (`dm_os_buffer_descriptors`, **nặng**) |
| **TempDB & dung lượng** | Dùng tempdb theo session (`dm_db_session_space_usage`); kích thước và dung lượng trống từng file (`sys.master_files`, `FILEPROPERTY`); dung lượng log (`DBCC SQLPERF(LOGSPACE)`); `log_reuse_wait_desc` |
| **Index/thống kê** | Phân mảnh index (`dm_db_index_physical_stats` chế độ LIMITED, **nặng**); thống kê cũ (`dm_db_stats_properties`) |
| **Log hệ thống** | **SQL Server error log** (`sp_readerrorlog`, lọc theo chuỗi và khoảng thời gian); SQL Agent log; **deadlock** từ `system_health` (`sys.fn_xe_file_target_read_file`); lịch sử backup (`msdb.dbo.backupset`); lịch sử job (`msdb.dbo.sysjobhistory`); trang nghi hỏng (`msdb.dbo.suspect_pages`); lần `CHECKDB` thành công gần nhất (`DATABASEPROPERTYEX`) |
| **Phiên & kết nối** | Số session/connection theo login, host, DB (`dm_exec_sessions`, `dm_exec_connections`) |
| **Hiệu năng tổng** | Perf counter chính: Page Life Expectancy, Batch Requests/sec, Buffer cache hit ratio (`dm_os_performance_counters`) |

- Tham số cho phép: **TOP N** (1–500, mặc định 50), **khoảng thời gian** (từ/đến) cho log, **chuỗi lọc** cho error log; mọi tham số truyền bằng `SqlParameter`, không nối chuỗi (R7).
- Timeout: 30 giây cho script thường, 120 giây cho script **nặng** (R9); có nút Hủy. Truy vấn qua `SqlConnectionSession.QueryTableAsync`.
- Thiếu quyền hoặc phiên bản không hỗ trợ: báo rõ riêng cho script đó, không ảnh hưởng script khác (R11). DMV rỗng thì hiện "chưa có dữ liệu".

## 2. Ngoài phạm vi
- Sửa hoặc tự soạn script, lưu script người dùng, lập lịch tự chạy, cảnh báo ngưỡng, dashboard/biểu đồ, Query Store, Extended Events tự tạo session.
- Thao tác thay đổi như kill session, clear cache, tạo job (thuộc N3-6); gợi ý và sinh script index (N3-5).

## 3. File dự kiến (chốt lại khi hoàn thiện spec)
| Đường dẫn | Tạo/Sửa |
|---|---|
| `SystemManagement/Utils/SqlDiagnosticScripts.cs` | Tạo (danh mục script, mỗi script là một record: Id, Nhóm, Tên, Mô tả, SQL, tham số, cờ nặng, quyền cần) |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQLScriptLibrary.cs` + `.Designer.cs` | Tạo (tab: cây nhóm, tham số, lưới kết quả, Xuất CSV) |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQL.cs` | Sửa (gắn tab) |

## 4. Tiêu chí nghiệm thu (tóm tắt, sẽ chi tiết hóa)
- Mọi script trong danh mục **chỉ đọc** (không `INSERT/UPDATE/DELETE/EXEC` thay đổi), kiểm bằng review danh sách SQL; chạy được khi đang Chỉ đọc.
- Mỗi script chạy được trên SQL Server 2019+ có quyền sysadmin (DB thử, 🔑); script thiếu quyền báo đúng cho script đó.
- Tham số `TOP N` ngoài 1–500 bị chặn; chuỗi lọc không gây SQL injection (kiểm bằng chuỗi có `'` và `;`).
- Script nặng hiện xác nhận Yes/No trước khi chạy; Hủy giữa chừng dừng truy vấn.
- Xuất CSV đúng tiêu đề và số dòng, escape dấu phẩy và xuống dòng; không ghi dữ liệu ra log (chỉ ghi tên script, thời gian, kết quả OK/FAIL).
- Logic danh mục và định dạng CSV kiểm bằng harness ngoài repo; chạy thật cần 🔑 SQL Server.
