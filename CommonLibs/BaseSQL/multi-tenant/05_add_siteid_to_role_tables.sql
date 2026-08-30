/*
  Thêm SiteId (denormalize từ tbl_Roles) vào tbl_Role_Function và tbl_User_Role - để lọc
  theo Site không cần join qua tbl_Roles, và để bật Row-Level Security ở bước sau
  (06_enable_row_level_security.sql). tbl_Roles đã có sẵn SiteId từ trước, không đổi gì
  ở bảng đó.

  Ghi chú kiểu dữ liệu: tbl_Roles.SiteId trong BaseSQL.sql gốc khai báo VARCHAR(50) (khác
  với tbl_Site.SiteId là VARCHAR(20) - lệch kiểu có sẵn từ trước trong schema gốc, không
  phải lỗi phát sinh ở migration này). Hai cột mới dưới đây dùng VARCHAR(50) cho khớp
  đúng kiểu nguồn copy (tbl_Roles.SiteId) - cân nhắc chuẩn hoá cả 2 về VARCHAR(20) trong
  một đợt dọn dẹp riêng nếu muốn, ngoài phạm vi file này.

  Convention: không dùng FOREIGN KEY (khớp schema gốc), chỉ dùng INDEX.
*/

USE BaseRepo;
GO

-- ===== tbl_Role_Function =====
IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Role_Function') AND name = 'SiteId'
)
BEGIN
    ALTER TABLE [dbo].[tbl_Role_Function] ADD [SiteId] VARCHAR(50) NULL;
END
GO

UPDATE rf
SET rf.SiteId = r.SiteId
FROM tbl_Role_Function rf
INNER JOIN tbl_Roles r ON r.RoleId = rf.RoleId
WHERE rf.SiteId IS NULL;
GO

IF EXISTS (SELECT 1 FROM tbl_Role_Function WHERE SiteId IS NULL)
BEGIN
    RAISERROR('tbl_Role_Function còn dòng không backfill được SiteId - có RoleId mồ côi (không còn tồn tại trong tbl_Roles), rà lại trước khi ép NOT NULL.', 16, 1);
    RETURN;
END
GO

ALTER TABLE [dbo].[tbl_Role_Function] ALTER COLUMN [SiteId] VARCHAR(50) NOT NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_tbl_Role_Function_SiteId' AND object_id = OBJECT_ID('dbo.tbl_Role_Function')
)
BEGIN
    CREATE INDEX [IX_tbl_Role_Function_SiteId] ON [dbo].[tbl_Role_Function]([SiteId]);
END
GO

-- ===== tbl_User_Role =====
IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_User_Role') AND name = 'SiteId'
)
BEGIN
    ALTER TABLE [dbo].[tbl_User_Role] ADD [SiteId] VARCHAR(50) NULL;
END
GO

UPDATE ur
SET ur.SiteId = r.SiteId
FROM tbl_User_Role ur
INNER JOIN tbl_Roles r ON r.RoleId = ur.RoleId
WHERE ur.SiteId IS NULL;
GO

IF EXISTS (SELECT 1 FROM tbl_User_Role WHERE SiteId IS NULL)
BEGIN
    RAISERROR('tbl_User_Role còn dòng không backfill được SiteId - có RoleId mồ côi (không còn tồn tại trong tbl_Roles), rà lại trước khi ép NOT NULL.', 16, 1);
    RETURN;
END
GO

ALTER TABLE [dbo].[tbl_User_Role] ALTER COLUMN [SiteId] VARCHAR(50) NOT NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_tbl_User_Role_SiteId' AND object_id = OBJECT_ID('dbo.tbl_User_Role')
)
BEGIN
    CREATE INDEX [IX_tbl_User_Role_SiteId] ON [dbo].[tbl_User_Role]([SiteId]);
END
GO

-- ===== Sanity check (chỉ đọc, không tự sửa) =====
-- Ràng buộc nghiệp vụ "Role.SiteId phải khớp User.SiteId" nay áp dụng ở tầng ứng dụng khi
-- CreateUserRole (thay cho điều kiện "phải có UserSite active" đã bỏ ở bước 02). Câu dưới
-- đây rà dữ liệu CŨ có sẵn hàng nào vi phạm hay không - dùng khoá (UserId, SiteId) đã
-- unique từ bước 02 để so khớp chính xác, KHÔNG join chỉ theo UserId vì UserId không còn
-- duy nhất toàn cục.
SELECT ur.AutoId, ur.UserId, ur.RoleId, ur.SiteId AS RoleSiteId
FROM tbl_User_Role ur
WHERE NOT EXISTS (
    SELECT 1 FROM tbl_Users u
    WHERE u.UserId = ur.UserId AND u.SiteId = ur.SiteId
);
GO
