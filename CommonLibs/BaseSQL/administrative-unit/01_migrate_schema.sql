/*
  Migration: bỏ cấp Quận/Huyện khỏi schema AdministrativeUnit.

  Lý do: từ 01/07/2025, Việt Nam sáp nhập đơn vị hành chính theo Nghị quyết 202/2025/QH15
  của Quốc hội — cả nước còn 34 tỉnh/thành, bỏ hẳn cấp Quận/Huyện, chỉ còn 2 cấp
  Tỉnh -> Xã/Phường trực tiếp. Bảng [District] không còn phản ánh đúng thực tế và không có
  dữ liệu "đơn vị hành chính mới" nào để insert vào đó.

  Script này:
  1. Xoá bảng [District] (nếu tồn tại).
  2. Đổi [Ward].[DistrictCode] -> [Ward].[ProvinceCode] (Ward giờ thuộc thẳng Province).

  ⚠️ Đây là thao tác có tính phá huỷ (DROP TABLE, DROP COLUMN). Nếu [District]/[Ward] trong DB
  của bạn đang có dữ liệu thật (không phải chỉ mới CREATE TABLE theo schema gửi kèm), hãy backup
  trước khi chạy — dữ liệu Quận/Huyện cũ và Ward theo DistrictCode cũ sẽ mất, vì bộ mã Xã/Phường
  mới hoàn toàn khác bộ mã cũ (không có bảng ánh xạ 1-1 an toàn để tự động chuyển đổi).

  Giả định: script này chạy trên schema đúng như bạn đã gửi (Province/District/Ward chưa có
  FOREIGN KEY constraint nào, chỉ có PK + non-clustered index) — nên không cần gỡ FK trước khi
  DROP.
*/

USE AdministrativeUnit;
GO

-- 1) Xoá bảng District
IF OBJECT_ID('dbo.District', 'U') IS NOT NULL
    DROP TABLE [dbo].[District];
GO

-- 2) Ward: bỏ DistrictCode, thêm ProvinceCode
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_District_ProvinceCode' AND object_id = OBJECT_ID('dbo.District')
)
BEGIN
    DROP INDEX [IX_District_ProvinceCode] ON [dbo].[District];
END
GO

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Ward_DistrictCode' AND object_id = OBJECT_ID('dbo.Ward')
)
BEGIN
    DROP INDEX [IX_Ward_DistrictCode] ON [dbo].[Ward];
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Ward') AND name = 'DistrictCode'
)
BEGIN
    ALTER TABLE [dbo].[Ward] DROP COLUMN [DistrictCode];
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Ward') AND name = 'ProvinceCode'
)
BEGIN
    -- Ward hiện đang rỗng (chưa có dữ liệu thật cho cơ cấu hành chính mới) nên thêm cột
    -- NOT NULL trực tiếp không cần default tạm. Nếu bảng của bạn đang có sẵn dữ liệu, câu lệnh
    -- này sẽ báo lỗi — cần TRUNCATE TABLE [dbo].[Ward] trước (xem ghi chú trong README.md).
    ALTER TABLE [dbo].[Ward] ADD [ProvinceCode] VARCHAR(20) NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Ward_ProvinceCode' AND object_id = OBJECT_ID('dbo.Ward')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Ward_ProvinceCode] ON [dbo].[Ward] ([ProvinceCode]);
END
GO
