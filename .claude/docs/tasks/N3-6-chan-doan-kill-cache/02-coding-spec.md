# N3-6 — ConfigSQL: chẩn đoán treo, kill session, clear cache, SQL Agent job · Coding Spec (bản phác thảo)

- **Trạng thái duyệt:** ⬜ chờ duyệt — **bản phác thảo**, BA bổ sung chi tiết và xin duyệt lại trước khi `/task-run`; có thể tách (chẩn đoán đọc | thao tác kill/cache/job) khi viết chi tiết
- **Yêu cầu gốc:** `../N3-configsql-quan-tri-sql-server/01-requirement.md` mục 4 (bước 8), R4, R5, R8, R10, R12, R14 · **Phụ thuộc: N3-1**

## 1. Phạm vi
Tab **Chẩn đoán** (nhóm 5):
- **Chẩn đoán (chỉ đọc):** blocking chain (ai chặn ai, `blocking_session_id`), session đang chạy lâu/đang chờ (`sys.dm_exec_requests`, `wait_type`, `wait_time`), transaction mở lâu (`sys.dm_tran_active_transactions`), tài nguyên CPU/memory. Làm mới theo nút (không polling).
- **Kill session** (`KILL <spid>`): chỉ khi bật Cho phép ghi; xác nhận mạnh gõ lại **SPID**; **chặn** kill chính session của app (`@@SPID` của mọi connection do app mở) và session hệ thống (`session_id` < 51 hoặc `is_user_process = 0`); báo nếu session đã kết thúc (R8). Chạy ngoài transaction (R14).
- **Clear cache** tức thời: `DBCC FREEPROCCACHE`, `DBCC DROPCLEANBUFFERS`, `DBCC FREESYSTEMCACHE('ALL')`; mỗi lệnh có cảnh báo tác động hiệu năng, xác nhận mạnh (gõ lại tên DB/lệnh), ngoài transaction.
- **SQL Agent job dọn cache định kỳ** (R12): tạo/xóa job tên tiền tố `BaseRepo_` qua `msdb.dbo.sp_add_job`, `sp_add_jobstep`, `sp_add_jobschedule`, `sp_add_jobserver` **trong một transaction** (R13); chỉ khi SQL Server Agent đang chạy; **không** đụng job không có tiền tố `BaseRepo_`; xem danh sách job `BaseRepo_*` hiện có.
- Mọi thao tác ghi/nguy hiểm ghi `Logs/ConfigSQL/`.

## 2. Ngoài phạm vi
- Kill hàng loạt tự động, tự phát hiện và kill, cảnh báo qua email, quản lý job không thuộc `BaseRepo_`, Extended Events.

## 3. File dự kiến (chốt lại khi hoàn thiện spec)
`SqlDiagnostics.cs` (truy vấn đọc), `SqlMaintenanceActions.cs` (kill, clear cache, job), `UC_ConfigSQLDiagnostics.cs` + `.Designer.cs`, `UC_ConfigSQL.cs` (gắn tab).

## 4. Tiêu chí nghiệm thu (tóm tắt, sẽ chi tiết hóa)
- Dựng blocking giả lập bằng hai connection trên DB thử; tab hiển thị đúng chuỗi chặn.
- Kill session thử thành công; kill `@@SPID` của app và session hệ thống bị chặn; kill SPID đã kết thúc báo đúng.
- Clear cache chạy sau xác nhận mạnh và ghi log; ở chế độ Chỉ đọc bị khóa.
- Tạo job `BaseRepo_ClearCache`: có trong `msdb`, **rollback hoàn toàn** nếu một bước lỗi (không còn job dở); Agent tắt thì báo rõ; xóa chỉ xóa job tiền tố `BaseRepo_`.
- Không log nội dung dữ liệu hay password; 🔑 SQL Server có Agent.
