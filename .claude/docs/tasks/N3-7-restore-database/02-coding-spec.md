# N3-7 — ConfigSQL: restore database · Coding Spec (bản phác thảo)

- **Trạng thái duyệt:** ⬜ chờ duyệt — **bản phác thảo**, BA bổ sung chi tiết và xin duyệt lại trước khi `/task-run`
- **Yêu cầu gốc:** `../N3-configsql-quan-tri-sql-server/01-requirement.md` mục 4 (bước 6), R5, R14, R16 · **Phụ thuộc: N3-4** (dùng chung tab Backup & Copy, `SqlBackupService`)

## 1. Phạm vi
Thêm **Restore database** (rủi ro cao nhất nên là task riêng), chạy ngoài transaction (R14):
- Chọn file `.bak` (đường dẫn trên máy SQL Server) → bắt buộc `RESTORE VERIFYONLY`, `RESTORE HEADERONLY` và `RESTORE FILELISTONLY` trước, hiển thị thông tin (tên DB gốc, ngày backup, danh sách file) và cho chọn `MOVE` file về đường dẫn mới.
- Khi đè DB đang có (`WITH REPLACE`): xác nhận mạnh gõ lại tên DB; **mặc định đề nghị backup DB hiện tại** (`COPY_ONLY`) trước và nêu rõ khi người dùng bỏ qua; nếu DB còn kết nối khác thì đề nghị đưa `SINGLE_USER WITH ROLLBACK IMMEDIATE` (có xác nhận mạnh), xong **trả lại `MULTI_USER`** kể cả khi restore lỗi.
- **Chặn** restore vào `master`, `model`, `msdb`, `tempdb`.
- Restore lỗi giữa chừng: báo DB có thể ở trạng thái `RESTORING`/không dùng được và gợi ý bước tiếp theo; ghi log `Logs/ConfigSQL/`.

## 2. Ngoài phạm vi
- Point-in-time restore, differential/log backup chain, restore từ URL/Azure, restore DB hệ thống.

## 3. File dự kiến (chốt lại khi hoàn thiện spec)
`SqlRestoreService.cs`, `UC_ConfigSQLBackupCopy.cs` (+ Designer, thêm phần Restore) hoặc `UC_ConfigSQLRestore.cs` + `.Designer.cs`.

## 4. Tiêu chí nghiệm thu (tóm tắt, sẽ chi tiết hóa)
- Restore file `.bak` thử vào DB mới có tên khác thành công, dữ liệu khớp.
- File hỏng hoặc `VERIFYONLY` lỗi: dừng trước khi đụng DB đích.
- Đè DB đang có cần gõ đúng tên DB; có backup `COPY_ONLY` trước nếu người dùng đồng ý; DB trở lại `MULTI_USER` sau cả khi thành công lẫn lỗi.
- Restore vào `master/model/msdb/tempdb` bị chặn; chế độ Chỉ đọc khóa nút; 🔑 SQL Server, chỉ DB thử.
