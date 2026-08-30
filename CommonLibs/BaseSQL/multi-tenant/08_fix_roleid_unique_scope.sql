/*
  Fix bị bỏ sót trong đợt migration multi-tenant trước: tbl_Roles.RoleId vẫn đang là UNIQUE
  INDEX toàn cục (IX_Roles_RoleId, từ schema gốc BaseSQL.sql) - CHƯA từng được đổi sang
  unique theo (SiteId, RoleId) ở bất kỳ script 01-07 nào. 05_add_siteid_to_role_tables.sql
  chỉ backfill cột SiteId cho tbl_Role_Function/tbl_User_Role, còn ghi rõ "tbl_Roles đã có
  sẵn SiteId từ trước, không đổi gì ở bảng đó" - nhưng lại quên luôn cả việc đổi UNIQUE INDEX
  của RoleId, y hệt lỗi mà 02_finalize_users_siteid.sql đã xử lý đúng cho UserId.

  Hệ quả nếu KHÔNG chạy file này: 2 Site khác nhau KHÔNG THỂ có Role trùng RoleId (VD cả
  2 Site cùng đặt RoleId 'ADMIN') - INSERT dòng thứ 2 sẽ bị SQL Server từ chối với lỗi vi
  phạm UNIQUE INDEX. Đây chính là kịch bản "RoleId trùng tên nhau giữa 2 Site" mà tài liệu
  thiết kế và README (mục "Không nằm trong các script này") yêu cầu phải test được - nói
  cách khác, trước khi chạy file này, kịch bản đó không thể xảy ra dù ở tầng ứng dụng đã
  sẵn sàng xử lý đúng (xem các fix trong UserRoleRepository/PermissionRepository/... cùng
  đợt), vì bị chặn ngay ở tầng DB.

  Cùng convention 02_finalize_users_siteid.sql: bỏ UNIQUE INDEX/CONSTRAINT đơn cột RoleId,
  tạo lại UNIQUE INDEX theo (SiteId, RoleId). Không dùng FOREIGN KEY (khớp toàn bộ schema
  gốc).

  ⚠️ Trước khi chạy: đảm bảo không có 2 dòng tbl_Roles nào hiện tại trùng (SiteId, RoleId)
  (không thể xảy ra vì RoleId đang unique toàn cục nên chắc chắn không trùng - bước kiểm
  tra dưới đây chỉ để phòng hờ nếu dữ liệu đã bị chỉnh sửa thủ công ngoài quy trình).
*/

USE BaseRepo;
GO

IF EXISTS (
    SELECT SiteId, RoleId FROM tbl_Roles
    GROUP BY SiteId, RoleId
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR('tbl_Roles đang có dòng trùng (SiteId, RoleId) - rà lại dữ liệu trước khi đổi unique index.', 16, 1);
    RETURN;
END
GO

-- 1) Bỏ constraint UNIQUE inline (tên tự sinh, tra theo sys.key_constraints) nếu có
DECLARE @ConstraintName SYSNAME;
SELECT @ConstraintName = kc.name
FROM sys.key_constraints kc
INNER JOIN sys.index_columns ic
    ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
INNER JOIN sys.columns c
    ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE kc.parent_object_id = OBJECT_ID('dbo.tbl_Roles')
  AND kc.[type] = 'UQ'
  AND c.name = 'RoleId'
  AND ic.index_column_id = 1;

IF @ConstraintName IS NOT NULL
BEGIN
    DECLARE @DropConstraintSql NVARCHAR(300) =
        N'ALTER TABLE [dbo].[tbl_Roles] DROP CONSTRAINT [' + @ConstraintName + N'];';
    PRINT N'Dropping inline UNIQUE constraint: ' + @ConstraintName;
    EXEC sp_executesql @DropConstraintSql;
END
GO

-- 2) Bỏ unique index đặt tên riêng (IX_Roles_RoleId, từ BaseSQL.sql)
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Roles_RoleId' AND object_id = OBJECT_ID('dbo.tbl_Roles')
)
BEGIN
    DROP INDEX [IX_Roles_RoleId] ON [dbo].[tbl_Roles];
END
GO

-- 3) Tạo lại UNIQUE INDEX theo (SiteId, RoleId) - mỗi Site tự quản RoleId độc lập
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Roles_SiteId_RoleId' AND object_id = OBJECT_ID('dbo.tbl_Roles')
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_SiteId_RoleId] ON [dbo].[tbl_Roles]([SiteId], [RoleId]);
END
GO

-- IX_Roles_ScreenId (tên đặt nhầm từ BaseSQL.sql gốc, thực chất index trên cột SiteId) đã
-- tồn tại sẵn từ trước, không cần tạo lại - vẫn hữu ích cho lookup theo SiteId.
