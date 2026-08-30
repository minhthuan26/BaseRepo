/*
  Seed fixture cho test cách ly 2 Site (xem README mục "Không nằm trong các script này" ->
  "Viết test cách ly 2 Site" và tests/test_two_site_isolation.sh trong cùng thư mục).

  Tạo 2 Site test độc lập, MỖI Site có 1 User và 1 Role dùng CHUNG (trùng) UserId/RoleId với
  Site kia - đúng kịch bản mà thiết kế multi-tenant phải cách ly đúng:
    - UserId "isotest_user" tồn tại ở CẢ 2 Site, là 2 User hoàn toàn khác nhau (khác AutoId).
    - RoleId "ISOTEST_ROLE" tồn tại ở CẢ 2 Site (chỉ khả thi sau khi chạy
      08_fix_roleid_unique_scope.sql - trước đó RoleId vẫn unique toàn cục, dòng INSERT thứ 2
      sẽ bị SQL Server từ chối).
    - Role của Site A được cấp Function "ToggleSiteModule", Role của Site B thì KHÔNG - dùng để
      test RequireFunction không bị "Admin Site A tự động có quyền ở Site B".

  Toàn bộ object test dùng tiền tố ISOTEST_ (Site/Role) hoặc isotest_ (User) để dễ nhận diện
  và dọn dẹp bằng cleanup_two_site_isolation.sql - KHÔNG đụng tới dữ liệu thật nào khác.

  ⚠️ Yêu cầu trước khi chạy:
    1. Đã chạy xong toàn bộ 00-08 theo đúng thứ tự trong README.md (đặc biệt 08 - bắt buộc để
       RoleId được phép trùng giữa 2 Site).
    2. Phải có sẵn ít nhất 1 tài khoản đang hoạt động trong tbl_Users để mượn ciphertext mật
       khẩu (mặc định dùng UserId 'admin', đúng tài khoản test chuẩn của repo - xem
       .claude/rules/06-testing.md, password 'admin2604'). Lý do: EncryptionUtils.EncryptData
       là mã hoá 2 chiều bằng khoá ứng dụng cố định (ApplicationName/ApplicationId trong
       appsettings.json) - không thể tạo ciphertext hợp lệ bằng T-SQL thuần, nên script mượn
       nguyên xi cột Password của 1 tài khoản đang đăng nhập được để 2 User test cũng dùng
       được đúng plaintext password đó ('admin2604') khi test.
    3. Phải có sẵn ít nhất 1 Module đang active trong tbl_Modules (để seed tbl_Site_Module cho
       phần test SiteModuleController) - nếu DB trống hoàn toàn, đoạn seed tbl_Site_Module bên
       dưới sẽ bị bỏ qua (không lỗi) và test_two_site_isolation.sh sẽ tự bỏ qua nhóm test đó.

  Convention: không dùng FOREIGN KEY (khớp toàn bộ schema gốc BaseSQL.sql).
*/

USE BaseRepo;
GO

DECLARE @BorrowedPassword VARCHAR(255) = (SELECT TOP 1 Password FROM tbl_Users WHERE UserId = 'admin' AND Status = 1);
IF @BorrowedPassword IS NULL
BEGIN
    RAISERROR('Không tìm thấy tài khoản UserId=''admin'' đang active để mượn ciphertext mật khẩu - xem ghi chú đầu file. Seed 1 tài khoản admin/admin2604 (qua API CreateUser) trước, hoặc sửa @BorrowedPassword thủ công.', 16, 1);
    RETURN;
END
GO

-- ===== 1) 2 Site test =====
DECLARE @BorrowedPassword VARCHAR(255) = (SELECT TOP 1 Password FROM tbl_Users WHERE UserId = 'admin' AND Status = 1);

IF NOT EXISTS (SELECT 1 FROM tbl_Site WHERE SiteId = 'ISOTEST_A')
BEGIN
    INSERT INTO tbl_Site (SiteId, SiteName, Status, CreatedDate, CreatedBy)
    VALUES ('ISOTEST_A', N'[ISOTEST] Site A', 1, SYSUTCDATETIME(), 'seed_script');
END

IF NOT EXISTS (SELECT 1 FROM tbl_Site WHERE SiteId = 'ISOTEST_B')
BEGIN
    INSERT INTO tbl_Site (SiteId, SiteName, Status, CreatedDate, CreatedBy)
    VALUES ('ISOTEST_B', N'[ISOTEST] Site B', 1, SYSUTCDATETIME(), 'seed_script');
END
GO

-- ===== 2) User "isotest_user" ở CẢ 2 Site (trùng UserId, khác AutoId/SiteId) =====
DECLARE @BorrowedPassword VARCHAR(255) = (SELECT TOP 1 Password FROM tbl_Users WHERE UserId = 'admin' AND Status = 1);

IF NOT EXISTS (SELECT 1 FROM tbl_Users WHERE UserId = 'isotest_user' AND SiteId = 'ISOTEST_A')
BEGIN
    INSERT INTO tbl_Users (UserId, SiteId, Password, UserName, Priority, Status, CreatedDate, CreatedBy)
    VALUES ('isotest_user', 'ISOTEST_A', @BorrowedPassword, N'[ISOTEST] User A', 1, 1, SYSUTCDATETIME(), 'seed_script');
END

IF NOT EXISTS (SELECT 1 FROM tbl_Users WHERE UserId = 'isotest_user' AND SiteId = 'ISOTEST_B')
BEGIN
    INSERT INTO tbl_Users (UserId, SiteId, Password, UserName, Priority, Status, CreatedDate, CreatedBy)
    VALUES ('isotest_user', 'ISOTEST_B', @BorrowedPassword, N'[ISOTEST] User B', 1, 1, SYSUTCDATETIME(), 'seed_script');
END
GO

-- ===== 3) Role "ISOTEST_ROLE" ở CẢ 2 Site (trùng RoleId - cần 08_fix_roleid_unique_scope.sql) =====
IF NOT EXISTS (SELECT 1 FROM tbl_Roles WHERE RoleId = 'ISOTEST_ROLE' AND SiteId = 'ISOTEST_A')
BEGIN
    INSERT INTO tbl_Roles (RoleId, RoleName, SiteId, Status, CreatedDate, CreatedBy)
    VALUES ('ISOTEST_ROLE', N'[ISOTEST] Role A', 'ISOTEST_A', 1, SYSUTCDATETIME(), 'seed_script');
END

IF NOT EXISTS (SELECT 1 FROM tbl_Roles WHERE RoleId = 'ISOTEST_ROLE' AND SiteId = 'ISOTEST_B')
BEGIN
    INSERT INTO tbl_Roles (RoleId, RoleName, SiteId, Status, CreatedDate, CreatedBy)
    VALUES ('ISOTEST_ROLE', N'[ISOTEST] Role B', 'ISOTEST_B', 1, SYSUTCDATETIME(), 'seed_script');
END
GO

-- ===== 4) Gán User <-> Role trong đúng Site của mình =====
IF NOT EXISTS (SELECT 1 FROM tbl_User_Role WHERE UserId = 'isotest_user' AND RoleId = 'ISOTEST_ROLE' AND SiteId = 'ISOTEST_A')
BEGIN
    INSERT INTO tbl_User_Role (UserId, RoleId, SiteId, Status, CreatedDate, CreatedBy)
    VALUES ('isotest_user', 'ISOTEST_ROLE', 'ISOTEST_A', 1, SYSUTCDATETIME(), 'seed_script');
END

IF NOT EXISTS (SELECT 1 FROM tbl_User_Role WHERE UserId = 'isotest_user' AND RoleId = 'ISOTEST_ROLE' AND SiteId = 'ISOTEST_B')
BEGIN
    INSERT INTO tbl_User_Role (UserId, RoleId, SiteId, Status, CreatedDate, CreatedBy)
    VALUES ('isotest_user', 'ISOTEST_ROLE', 'ISOTEST_B', 1, SYSUTCDATETIME(), 'seed_script');
END
GO

-- ===== 5) Function cấp LỆCH giữa 2 Site - Site A có "ToggleSiteModule" và "CreateUserRole",
--          Site B chỉ có "CreateUserRole" (KHÔNG có "ToggleSiteModule") =====
-- Không cần tồn tại catalog tbl_Functions tương ứng - AuthorizationMiddleware chỉ đọc thẳng
-- tbl_Role_Function khi tính allowedFunctionIds (Bước 8, xem CommonLibs/Middlewares/AuthorizationMiddleware.cs),
-- không join qua tbl_Functions.
IF NOT EXISTS (SELECT 1 FROM tbl_Role_Function WHERE RoleId = 'ISOTEST_ROLE' AND SiteId = 'ISOTEST_A' AND FunctionId = 'ToggleSiteModule')
BEGIN
    INSERT INTO tbl_Role_Function (RoleId, FunctionId, SiteId, Status, CreatedDate, CreatedBy)
    VALUES ('ISOTEST_ROLE', 'ToggleSiteModule', 'ISOTEST_A', 1, SYSUTCDATETIME(), 'seed_script');
END

IF NOT EXISTS (SELECT 1 FROM tbl_Role_Function WHERE RoleId = 'ISOTEST_ROLE' AND SiteId = 'ISOTEST_A' AND FunctionId = 'CreateUserRole')
BEGIN
    INSERT INTO tbl_Role_Function (RoleId, FunctionId, SiteId, Status, CreatedDate, CreatedBy)
    VALUES ('ISOTEST_ROLE', 'CreateUserRole', 'ISOTEST_A', 1, SYSUTCDATETIME(), 'seed_script');
END

IF NOT EXISTS (SELECT 1 FROM tbl_Role_Function WHERE RoleId = 'ISOTEST_ROLE' AND SiteId = 'ISOTEST_B' AND FunctionId = 'CreateUserRole')
BEGIN
    INSERT INTO tbl_Role_Function (RoleId, FunctionId, SiteId, Status, CreatedDate, CreatedBy)
    VALUES ('ISOTEST_ROLE', 'CreateUserRole', 'ISOTEST_B', 1, SYSUTCDATETIME(), 'seed_script');
END
-- CHỦ Ý: Site B KHÔNG được cấp "ToggleSiteModule" - dùng để test RequireFunction chặn đúng,
-- không bị rò quyền chéo Site chỉ vì 2 Role trùng RoleId "ISOTEST_ROLE".
GO

-- ===== 6) tbl_Site_Module: bật 1 Module có sẵn cho Site A, CỐ Ý KHÔNG bật cho Site B =====
-- Mượn Module đầu tiên đang active trong hệ thống (không tạo Module mới, không đụng dữ liệu
-- thật khác). Nếu DB chưa có Module nào, đoạn này không làm gì cả (test script sẽ tự bỏ qua
-- nhóm test SiteModuleController).
DECLARE @BorrowedModuleId VARCHAR(50) = (SELECT TOP 1 ModuleId FROM tbl_Modules WHERE Status = 1 ORDER BY AutoId ASC);
IF @BorrowedModuleId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tbl_Site_Module WHERE SiteId = 'ISOTEST_A' AND ModuleId = @BorrowedModuleId)
    BEGIN
        INSERT INTO tbl_Site_Module (SiteId, ModuleId, Status, CreatedDate, CreatedBy)
        VALUES ('ISOTEST_A', @BorrowedModuleId, 1, SYSUTCDATETIME(), 'seed_script');
    END
    -- Site B: KHÔNG insert dòng tbl_Site_Module nào cho Module này - ToggleSiteModule phải trả
    -- 404 ("chưa được đội vận hành cấp") ngay cả khi giả sử Role của Site B có Function này.
END
GO

PRINT 'Seed hoàn tất. Kiểm tra nhanh:';
SELECT UserId, SiteId, AutoId, UserName FROM tbl_Users WHERE UserId = 'isotest_user' ORDER BY SiteId;
SELECT RoleId, SiteId, RoleName FROM tbl_Roles WHERE RoleId = 'ISOTEST_ROLE' ORDER BY SiteId;
SELECT RoleId, SiteId, FunctionId FROM tbl_Role_Function WHERE RoleId = 'ISOTEST_ROLE' ORDER BY SiteId, FunctionId;
SELECT SiteId, ModuleId, Status FROM tbl_Site_Module WHERE SiteId IN ('ISOTEST_A', 'ISOTEST_B');
GO
