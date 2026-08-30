/*
  Tạo bảng tbl_Site_Module (entitlement + bật/tắt Module theo Site) và backfill từ quan hệ
  Module.SiteId cũ, TRƯỚC KHI xoá cột SiteId khỏi tbl_Modules ở bước tiếp theo
  (04_globalize_module_catalog.sql) - thứ tự bắt buộc, vì backfill cần đọc Module.SiteId
  trong khi cột đó vẫn còn tồn tại.

  Ý nghĩa dữ liệu sau backfill: mỗi Site tiếp tục có đúng những Module đang dùng hôm nay,
  Status = 1 (bật) cho toàn bộ - không Site nào bị mất quyền truy cập Module đang chạy.
  "Mặc định tắt" (không có dòng nào) chỉ áp dụng cho Module MỚI ra mắt sau thời điểm này.

  Hai thao tác khác nhau trên bảng này (xem tài liệu thiết kế, mục 04+07):
    - Tạo dòng (provisioning) = việc của đội vận hành khi Site mua thêm Module, KHÔNG
      phải API Site Admin gọi được. Ở bản on-prem, việc này tương đương "không chạy seed
      cho Module chưa mua". Ở bản thuê trọn bộ (nhiều Site chung 1 cụm hạ tầng), việc chỉ
      tạo dòng khi đã mua chính là tương đương của "không deploy" trong bản on-prem.
    - Bật/tắt Status trong các dòng đã tồn tại = quyền Site Admin bình thường.

  Convention: không dùng FOREIGN KEY (khớp toàn bộ schema gốc BaseSQL.sql), chỉ dùng
  UNIQUE INDEX.
*/

USE BaseRepo;
GO

IF OBJECT_ID('dbo.tbl_Site_Module', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[tbl_Site_Module] (
        [AutoId] BIGINT IDENTITY(1,1) PRIMARY KEY
        ,[SiteId] VARCHAR(20) NOT NULL
        ,[ModuleId] VARCHAR(50) NOT NULL
        ,[CreatedDate] DATETIME2 NULL
        ,[UpdatedDate] DATETIME2 NULL
        ,[CreatedBy] NVARCHAR(20) NULL
        ,[UpdatedBy] NVARCHAR(20) NULL
        ,[Status] INT NOT NULL DEFAULT 1
        ,[DeviceName] NVARCHAR(255) NULL
        ,[DeviceId] NVARCHAR(255) NULL
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_tbl_Site_Module_SiteId_ModuleId' AND object_id = OBJECT_ID('dbo.tbl_Site_Module')
)
BEGIN
    CREATE UNIQUE INDEX [IX_tbl_Site_Module_SiteId_ModuleId]
        ON [dbo].[tbl_Site_Module]([SiteId], [ModuleId]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_tbl_Site_Module_SiteId' AND object_id = OBJECT_ID('dbo.tbl_Site_Module')
)
BEGIN
    CREATE INDEX [IX_tbl_Site_Module_SiteId] ON [dbo].[tbl_Site_Module]([SiteId]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_tbl_Site_Module_ModuleId' AND object_id = OBJECT_ID('dbo.tbl_Site_Module')
)
BEGIN
    CREATE INDEX [IX_tbl_Site_Module_ModuleId] ON [dbo].[tbl_Site_Module]([ModuleId]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_tbl_Site_Module_Status_CreatedDate_UpdatedDate' AND object_id = OBJECT_ID('dbo.tbl_Site_Module')
)
BEGIN
    CREATE INDEX [IX_tbl_Site_Module_Status_CreatedDate_UpdatedDate]
        ON [dbo].[tbl_Site_Module]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
END
GO

-- Backfill: Module hiện đang gắn SiteId nào -> bật sẵn (Status = 1) cho đúng Site đó,
-- để không Site nào mất quyền dùng Module đang chạy khi cột SiteId bị xoá ở bước sau.
INSERT INTO [dbo].[tbl_Site_Module] ([SiteId], [ModuleId], [Status], [CreatedDate])
SELECT m.SiteId, m.ModuleId, 1, SYSUTCDATETIME()
FROM tbl_Modules m
WHERE NOT EXISTS (
    SELECT 1 FROM tbl_Site_Module sm
    WHERE sm.SiteId = m.SiteId AND sm.ModuleId = m.ModuleId
);
GO

-- Đối chiếu: số dòng backfill nên bằng đúng số dòng tbl_Modules hiện có (mỗi Module chỉ
-- gắn 1 SiteId trong schema cũ, nên 1-1 với tbl_Site_Module sau backfill).
SELECT
    (SELECT COUNT(*) FROM tbl_Modules) AS TotalModulesBeforeGlobalize
    ,(SELECT COUNT(*) FROM tbl_Site_Module) AS TotalSiteModuleRows;
GO
