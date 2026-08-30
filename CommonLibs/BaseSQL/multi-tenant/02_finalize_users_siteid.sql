/*
  Bước 2/2 gắn SiteId vào tbl_Users: khoá cứng NOT NULL, đổi UserId từ unique toàn cục
  sang unique theo từng Site, xoá bảng trung gian tbl_User_Site.

  ⚠️ Có DROP TABLE tbl_User_Site - thao tác phá huỷ. Chỉ chạy sau khi:
    1. Đã chạy 01_add_siteid_to_users.sql và xác nhận KHÔNG còn user nào SiteId IS NULL
       (câu SELECT cuối file 01 phải trả về 0 dòng - script này tự kiểm tra lại và dừng
       nếu chưa đạt điều kiện).
    2. Đã backup tbl_User_Site nếu muốn giữ lịch sử user từng thuộc Site nào trước đây.

  Ghi chú convention: schema gốc (BaseSQL.sql) không dùng FOREIGN KEY constraint ở đâu cả,
  chỉ dùng UNIQUE INDEX + app tự đảm bảo tính toàn vẹn - file này giữ đúng convention đó,
  không thêm FK cho SiteId dù về mặt kỹ thuật có thể thêm.
*/

USE BaseRepo;
GO

IF EXISTS (SELECT 1 FROM tbl_Users WHERE SiteId IS NULL)
BEGIN
    RAISERROR('Còn user chưa có SiteId - chạy lại phần cuối 01_add_siteid_to_users.sql và gán tay cho hết trước khi chạy file này.', 16, 1);
    RETURN;
END
GO

ALTER TABLE [dbo].[tbl_Users] ALTER COLUMN [SiteId] VARCHAR(20) NOT NULL;
GO

-- tbl_Users.UserId trong BaseSQL.sql gốc có UNIQUE inline (constraint tên tự sinh, dạng
-- UQ__tbl_Users__...) CỘNG THÊM 1 unique index đặt tên riêng IX_Users_UserId - cả hai
-- cùng enforce unique toàn cục, cần bỏ CẢ HAI để đổi sang unique theo (SiteId, UserId).

-- 2a) Bỏ constraint UNIQUE inline (tên tự sinh, tra theo sys.key_constraints)
DECLARE @ConstraintName SYSNAME;
SELECT @ConstraintName = kc.name
FROM sys.key_constraints kc
INNER JOIN sys.index_columns ic
    ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
INNER JOIN sys.columns c
    ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE kc.parent_object_id = OBJECT_ID('dbo.tbl_Users')
  AND kc.[type] = 'UQ'
  AND c.name = 'UserId'
  AND ic.index_column_id = 1;                 -- chỉ lấy constraint đơn cột UserId

IF @ConstraintName IS NOT NULL
BEGIN
    DECLARE @DropConstraintSql NVARCHAR(300) =
        N'ALTER TABLE [dbo].[tbl_Users] DROP CONSTRAINT [' + @ConstraintName + N'];';
    PRINT N'Dropping inline UNIQUE constraint: ' + @ConstraintName;
    EXEC sp_executesql @DropConstraintSql;
END
GO

-- 2b) Bỏ unique index đặt tên riêng
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Users_UserId' AND object_id = OBJECT_ID('dbo.tbl_Users')
)
BEGIN
    DROP INDEX [IX_Users_UserId] ON [dbo].[tbl_Users];
END
GO

-- 2c) Tạo lại unique index theo (SiteId, UserId)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Users_SiteId_UserId' AND object_id = OBJECT_ID('dbo.tbl_Users')
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_SiteId_UserId] ON [dbo].[tbl_Users]([SiteId], [UserId]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Users_SiteId' AND object_id = OBJECT_ID('dbo.tbl_Users')
)
BEGIN
    CREATE INDEX [IX_Users_SiteId] ON [dbo].[tbl_Users]([SiteId]);
END
GO

-- 3) Xoá bảng trung gian - quan hệ giờ là 1 User = 1 Site trực tiếp qua cột SiteId,
--    không còn cần bảng N-N nữa.
IF OBJECT_ID('dbo.tbl_User_Site', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[tbl_User_Site];
END
GO
