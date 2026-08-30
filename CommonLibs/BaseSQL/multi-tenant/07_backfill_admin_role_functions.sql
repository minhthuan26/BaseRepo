/*
  Backfill Function cho mọi Role tên "Admin" - BẮT BUỘC chạy cùng đợt deploy code đã bỏ cơ chế
  bypass "Admin toàn hệ thống" trong CommonLibs/Middlewares/AuthorizationMiddleware.cs.

  Bối cảnh: trước đây AuthorizationMiddleware cho phép bất kỳ user nào có Role tên "Admin" (so khớp
  literal, không phân biệt hoa/thường) bỏ qua mọi kiểm tra [RequireFunction(...)], bất kể Role "Admin"
  đó do Site nào cấp. Sau khi mỗi Site tự có Role "Admin" riêng (multi-tenant), hành vi này vô tình cho
  phép Admin của Site A bỏ qua kiểm tra quyền trên tài nguyên của Site B - đã BỎ bypass này trong code
  (xem AuthorizationMiddleware.cs, BaseController.IsSiteAdmin, và đã xoá "AdminPolicy" ở 3 Program.cs).

  HỆ QUẢ: ngay khi bỏ bypass, Role "Admin" của MỌI Site sẽ mất hết quyền thao tác đã hoạt động trước
  đó, TRỪ KHI tbl_Role_Function đã có sẵn đủ dòng cấp quyền tường minh. Script này backfill đúng 1 lần,
  cấp cho mọi Role tên "Admin" toàn bộ Function đang active (Status = 1) hiện có trong hệ thống - tương
  đương với đúng quyền hạn mà bypass cũ từng ngầm cấp - để không Site nào bị khoá quyền đột ngột ngay
  khi deploy đợt fix này.

  LƯU Ý: đây là backfill "grandfather" 1 lần cho đúng ngày cutover, KHÔNG phải khẳng định "Role Admin
  nên luôn có mọi Function mãi mãi". Sau khi deploy ổn định, cân nhắc rà lại và thu hẹp quyền của từng
  Role "Admin" qua chính API AssignRolePermissions (Site.Application/Features/Permission) nếu muốn quản
  trị theo nguyên tắc least-privilege chặt hơn - script này không tự làm việc đó.

  Thứ tự chạy: SAU 05_add_siteid_to_role_tables.sql (cần cột tbl_Role_Function.SiteId đã tồn tại và
  NOT NULL để insert đúng). Không phụ thuộc 06 (RLS) - chạy được dù RLS đang bật hay tắt.

  Convention: không dùng FOREIGN KEY (khớp schema gốc), chỉ dùng INDEX/UNIQUE INDEX sẵn có.
*/

USE BaseRepo;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Role_Function') AND name = 'SiteId'
)
BEGIN
    RAISERROR('tbl_Role_Function chưa có cột SiteId - chạy 05_add_siteid_to_role_tables.sql trước.', 16, 1);
    RETURN;
END
GO

-- Cấp toàn bộ Function đang active cho mọi Role tên "Admin" (mọi Site), bỏ qua dòng đã tồn tại sẵn.
INSERT INTO [dbo].[tbl_Role_Function] ([RoleId], [FunctionId], [SiteId], [Status], [CreatedDate])
SELECT r.RoleId, f.FunctionId, r.SiteId, 1, SYSUTCDATETIME()
FROM tbl_Roles r
CROSS JOIN tbl_Functions f
WHERE r.RoleName = N'Admin'
  AND r.Status = 1
  AND f.Status = 1
  AND NOT EXISTS (
      SELECT 1 FROM tbl_Role_Function rf
      WHERE rf.RoleId = r.RoleId AND rf.FunctionId = f.FunctionId
  );
GO

-- Đối chiếu: mỗi Role "Admin" đang active nên có đúng bằng tổng số Function đang active. Dòng nào lệch
-- (ShortBy > 0) nghĩa là còn thiếu Function - kiểm tra lại trước khi coi đợt backfill là hoàn tất.
SELECT
    r.RoleId
    ,r.SiteId
    ,r.RoleName
    ,(SELECT COUNT(*) FROM tbl_Role_Function rf WHERE rf.RoleId = r.RoleId AND rf.Status = 1) AS GrantedFunctionCount
    ,(SELECT COUNT(*) FROM tbl_Functions WHERE Status = 1) AS TotalActiveFunctionCount
    ,(SELECT COUNT(*) FROM tbl_Functions WHERE Status = 1)
        - (SELECT COUNT(*) FROM tbl_Role_Function rf WHERE rf.RoleId = r.RoleId AND rf.Status = 1) AS ShortBy
FROM tbl_Roles r
WHERE r.RoleName = N'Admin' AND r.Status = 1
ORDER BY r.SiteId;
GO
