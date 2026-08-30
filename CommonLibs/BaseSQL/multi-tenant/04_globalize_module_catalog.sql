/*
  Bỏ SiteId khỏi tbl_Modules - chuyển thành danh mục dùng chung toàn hệ thống thay vì lặp
  lại theo từng Site (xem tài liệu thiết kế, mục 04). tbl_Screens và tbl_Functions trong
  schema gốc (BaseSQL.sql) KHÔNG có cột SiteId trực tiếp (chỉ có ModuleId/ScreenId) - đã
  toàn cục sẵn từ trước một cách gián tiếp qua chuỗi Module -> Screen -> Function, nên
  không cần đổi gì ở 2 bảng đó. Hai khối IF ở cuối file chỉ là phòng hờ, sẽ không làm gì
  cả trên schema gốc - chỉ có tác dụng nếu DB thực tế của bạn đã tự thêm SiteId khác với
  schema gửi kèm.

  ⚠️ Bắt buộc đã chạy xong 03_create_site_module.sql TRƯỚC file này - backfill ở đó cần
  đọc tbl_Modules.SiteId trong khi cột còn tồn tại. Script này tự kiểm tra và dừng nếu
  chưa đạt điều kiện.

  ⚠️ Trước khi chạy: kiểm tra không có 2 Site nào đang định nghĩa cùng ý nghĩa nghiệp vụ
  nhưng khác ModuleId (VD Site A có Module "QuanLyNguoiDung", Site B có Module "UserMgmt"
  cho cùng 1 chức năng) - nếu có, phải gộp lại thủ công thành 1 bản ghi trước khi drop
  cột, nếu không sẽ có nhiều dòng trùng nghĩa nhưng khác mã tồn tại song song sau khi gộp.
  Đây là việc cần review dữ liệu bằng mắt, không có câu SQL nào tự phát hiện "cùng ý
  nghĩa nghiệp vụ" một cách an toàn.
*/

USE BaseRepo;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_Site_Module')
   OR (SELECT COUNT(*) FROM tbl_Site_Module) < (SELECT COUNT(*) FROM tbl_Modules)
BEGIN
    RAISERROR('Chưa chạy xong 03_create_site_module.sql (thiếu bảng hoặc backfill chưa đủ) - chạy file đó trước.', 16, 1);
    RETURN;
END
GO

-- tbl_Modules
IF EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_Modules_SiteId' AND object_id = OBJECT_ID('dbo.tbl_Modules')
)
BEGIN
    DROP INDEX [IX_Modules_SiteId] ON [dbo].[tbl_Modules];
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Modules') AND name = 'SiteId'
)
BEGIN
    ALTER TABLE [dbo].[tbl_Modules] DROP COLUMN [SiteId];
END
GO

-- tbl_Screens - phòng hờ, no-op trên schema gốc (không có cột SiteId để xoá)
IF EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Screens') AND name = 'SiteId'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Screens_SiteId' AND object_id = OBJECT_ID('dbo.tbl_Screens'))
        DROP INDEX [IX_Screens_SiteId] ON [dbo].[tbl_Screens];
    ALTER TABLE [dbo].[tbl_Screens] DROP COLUMN [SiteId];
END
GO

-- tbl_Functions - phòng hờ, no-op trên schema gốc (không có cột SiteId để xoá)
IF EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Functions') AND name = 'SiteId'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Functions_SiteId' AND object_id = OBJECT_ID('dbo.tbl_Functions'))
        DROP INDEX [IX_Functions_SiteId] ON [dbo].[tbl_Functions];
    ALTER TABLE [dbo].[tbl_Functions] DROP COLUMN [SiteId];
END
GO

-- ModuleId/ScreenId/FunctionId giữ nguyên UNIQUE toàn cục như hiện tại - không đổi gì,
-- vì giờ chúng đúng nghĩa là danh mục dùng chung, không còn lặp lại theo Site nữa.
