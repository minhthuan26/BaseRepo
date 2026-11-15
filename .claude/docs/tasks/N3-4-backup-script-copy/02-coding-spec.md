# N3-4 — ConfigSQL: backup, sinh script schema, copy schema và dữ liệu sang DB khác · Coding Spec (bản phác thảo)

- **Trạng thái duyệt:** ⬜ chờ duyệt — **bản phác thảo**, BA bổ sung chi tiết và xin duyệt lại trước khi `/task-run`; có thể tách thành 2 task nhỏ (backup + script | copy) khi viết chi tiết nếu vượt ~8 file
- **Yêu cầu gốc:** `../N3-configsql-quan-tri-sql-server/01-requirement.md` mục 4 (bước 6), R5, R13, R14, R15 · **Phụ thuộc: N3-1, N3-3** (dùng `SqlScriptGenerator`)

## 1. Phạm vi
Tab **Backup & Copy** (nhóm 3, trừ Restore):
- **Backup** `BACKUP DATABASE` (đường dẫn trên máy SQL Server, tùy chọn `COPY_ONLY`/`COMPRESSION`/`CHECKSUM`), chạy **ngoài transaction** (R14), có xác nhận mạnh, tiến độ và hủy.
- **Sinh script schema** của DB (bảng, khóa, index, view, SP, function, trigger) và **file/filegroup** (sinh script cấu trúc file/filegroup để tái tạo ở DB đích, không copy `.mdf/.ndf` vật lý).
- **Copy sang DB đích** (từ **một kết nối nguồn** sang **một hoặc nhiều kết nối đích**, ví dụ 1 nguồn và 3 đích, lấy từ danh sách kết nối của N3-1: đích phải Sẵn sàng, bật Cho phép ghi và **không trùng nguồn**; mỗi đích chạy **tuần tự, transaction riêng** như R20, lỗi một đích không rollback đích khác, tùy chọn *Dừng khi một đích lỗi* mặc định bật; xác nhận mạnh gộp một hộp liệt kê mọi đích) với **3 chế độ chọn được (R17)**: **Chỉ schema** (chỉ tạo đối tượng, không copy dòng), **Chỉ dữ liệu** (bảng đích phải có sẵn; kiểm tra tên bảng, tên/kiểu/độ dài cột, bỏ qua identity và computed; thiếu hoặc lệch thì dừng và liệt kê, không tự tạo; chọn *chèn thêm* hoặc *xóa dữ liệu cũ trước* có xác nhận mạnh, bảng có FK trỏ tới thì báo thay vì `TRUNCATE`, không tự tắt FK), **Cả hai** (tạo schema rồi copy dữ liệu). Tạo bảng ở đích (tự xử lý thứ tự FK), copy dữ liệu bằng `SqlBulkCopy`, `BatchSize` 5.000. Mỗi bảng chạy trong một transaction; bảng từ ngưỡng **1.000.000 dòng** (chỉnh được) chuyển sang commit từng lô, và nếu lỗi/hủy thì DROP bảng đích do lần chạy này tạo, còn bảng có sẵn thì dừng và báo số dòng đã chèn (R15). Bảng đích trùng tên: hỏi *Bỏ qua / Thay thế / Chỉ chèn thêm*, mặc định *Bỏ qua*. Hiện chế độ (một transaction hay từng lô) cho từng bảng trước khi chạy.

## 2. Ngoài phạm vi
- Restore (N3-7), differential/log backup, copy file vật lý, đồng bộ hai chiều, so sánh schema.

## 3. File dự kiến (chốt lại khi hoàn thiện spec)
`SqlBackupService.cs`, `SqlSchemaScripter.cs` (mở rộng `SqlScriptGenerator` của N3-3), `SqlDataCopier.cs` (copy từ nguồn sang nhiều đích, dùng `FrmSqlDangerConfirm` của N3-2; **không còn** form kết nối riêng cho DB đích), `UC_ConfigSQLBackupCopy.cs` + `.Designer.cs`, `UC_ConfigSQL.cs` (gắn tab).

## 4. Tiêu chí nghiệm thu (tóm tắt, sẽ chi tiết hóa)
- Backup tạo file trên server, `RESTORE VERIFYONLY` (chạy tay) pass; Chỉ đọc thì khóa nút.
- Script schema chạy trên DB rỗng tạo ra schema tương đương (so `sys.columns`/`sys.indexes` trước/sau).
- Copy bảng nhỏ: số dòng khớp; lỗi giữa chừng → rollback bảng, không còn bảng dở.
- **1 nguồn + 3 đích** (DB thử): tạo bảng từ nguồn sang cả 3 đích thành công, mỗi đích có cùng schema; một đích lỗi (ví dụ đã có bảng trùng) chỉ rollback đích đó, đích đã xong giữ nguyên, đích chưa chạy theo tùy chọn Dừng/Tiếp tục; chọn đích trùng nguồn bị chặn; nhật ký một dòng mỗi đích.
- **Chỉ schema:** đích có đủ bảng/khóa/index nhưng 0 dòng. **Chỉ dữ liệu:** đích thiếu bảng hoặc lệch cột thì dừng trước khi chép và liệt kê đúng chỗ lệch; chế độ *xóa dữ liệu cũ trước* nằm trong transaction của bảng, lỗi giữa chừng thì dữ liệu cũ còn nguyên; bảng có FK trỏ tới bị bỏ qua kèm lý do. **Cả hai:** schema rồi dữ liệu, số dòng khớp nguồn.
- Copy bảng lớn (giả lập ngưỡng thấp): commit từng lô; hủy → DROP bảng đích nếu do lần chạy tạo, nếu bảng có sẵn thì báo đúng số dòng đã chèn.
- Không log dữ liệu bảng, không log password kết nối đích; kiểm bằng DB thử (🔑 SQL Server).
