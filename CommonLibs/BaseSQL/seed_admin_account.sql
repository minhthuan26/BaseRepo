/*
  Seed đầy đủ quyền cho tài khoản chuẩn UserId 'admin' / password 'admin2604' (đúng tài khoản
  test đã ghi ở .claude/rules/06-testing.md) - tạo/đảm bảo có 1 Role được cấp TOÀN BỘ FunctionId
  đang được enforce qua [RequireFunction(...)] trong code hiện tại, rồi gán Role đó cho 'admin'.

  BỐI CẢNH: từ khi bỏ cơ chế bypass "Admin toàn hệ thống" (xem
  CommonLibs/BaseSQL/multi-tenant/README.md mục "Fix: bỏ bypass Admin toàn hệ thống"), Role tên
  "Admin" KHÔNG còn tự động có quyền gì - phải được cấp Function tường minh qua tbl_Role_Function
  như mọi Role khác, giống hệt cách 07_backfill_admin_role_functions.sql đã làm cho các Role
  "Admin" có sẵn. Script này làm việc tương tự nhưng target đúng 1 tài khoản admin cụ thể, và
  liệt kê tường minh danh sách FunctionId (không CROSS JOIN tbl_Functions, vì AuthorizationMiddleware
  chỉ đọc thẳng tbl_Role_Function khi tính allowedFunctionIds - không join qua tbl_Functions, nên
  catalog tbl_Functions có trống cũng không ảnh hưởng tới việc enforce quyền).

  Danh sách 39 FunctionId dưới đây lấy từ toàn bộ [RequireFunction("...")] đang có trong code (rà
  bằng grep tại thời điểm viết script - 2026-09-01). Nếu sau này thêm action mới có
  [RequireFunction(...)], phải bổ sung thủ công vào bảng @Functions bên dưới rồi chạy lại script.

  GIỚI HẠN QUAN TRỌNG - KHÔNG tự tạo dòng tbl_Users: EncryptionUtils.EncryptData
  (Gateway.Infrastructure/Repositories/UserRepository.cs) mã hoá password 2 chiều bằng khoá ứng
  dụng cố định (ApplicationName/ApplicationId trong appsettings.json) - KHÔNG thể tạo ciphertext
  hợp lệ bằng T-SQL thuần (xem cùng giới hạn đã ghi ở
  CommonLibs/BaseSQL/multi-tenant/tests/seed_two_site_isolation.sql). Script này CHỈ thao tác
  Site/Role/Role_Function/User_Role:
    - Nếu UserId 'admin' ĐÃ tồn tại (rất có thể đúng - đây là tài khoản test chuẩn của repo, đã
      được dùng xuyên suốt Progress Log trong claude.md): script tự dò đúng SiteId hiện có của
      user đó và cấp quyền ngay, không cần sửa gì.
    - Nếu UserId 'admin' CHƯA tồn tại (DB hoàn toàn mới): phải tạo tài khoản trước qua API
      CreateUser - nhưng CreateUser tự nó cũng đòi [RequireFunction("CreateUser")] (bootstrap
      dây chuyền, xem Gateway/Gateway.Api/Controllers/UserController.cs) nên KHÔNG gọi được nếu
      DB chưa có bất kỳ tài khoản nào. Trường hợp này cần nhờ tạo 1 helper nhỏ gọi thẳng
      EncryptionUtils.EncryptData để lấy ciphertext hợp lệ rồi INSERT tbl_Users thủ công - báo lại
      nếu rơi vào tình huống này, KHÔNG tự đoán ciphertext.

  Idempotent - chạy lại nhiều lần an toàn (mọi INSERT đều có kiểm tra tồn tại trước, tự reactivate
  dòng đã bị soft-delete Status <> 1 thay vì insert trùng và vi phạm unique index).

  Convention: không dùng FOREIGN KEY (khớp BaseSQL.sql gốc), chỉ dùng INDEX/UNIQUE INDEX sẵn có.
*/

USE BaseRepo;
GO

DECLARE @UserId    VARCHAR(20)   = 'admin';
DECLARE @RoleId    VARCHAR(20)   = 'ADMIN';
DECLARE @RoleName  NVARCHAR(255) = N'Admin';
DECLARE @CreatedBy NVARCHAR(20)  = 'seed_script';

-- Site mặc định CHỈ dùng khi UserId 'admin' chưa tồn tại ở bất kỳ Site nào (DB hoàn toàn mới) -
-- sửa lại cho khớp Site thật của bạn nếu không muốn dùng giá trị mặc định này.
DECLARE @DefaultSiteId   VARCHAR(20)   = 'MAIN';
DECLARE @DefaultSiteName NVARCHAR(255) = N'Trụ sở chính';

-- ============================================================================
-- 0) Xác định SiteId - ưu tiên dò theo tài khoản 'admin' đã có sẵn, tránh đoán sai Site
-- ============================================================================
DECLARE @SiteId VARCHAR(20);
SELECT TOP 1 @SiteId = SiteId FROM tbl_Users WHERE UserId = @UserId AND Status = 1;

DECLARE @UserExists BIT = IIF(@SiteId IS NOT NULL, 1, 0);
IF @SiteId IS NULL
BEGIN
    SET @SiteId = @DefaultSiteId;
    PRINT N'CẢNH BÁO: chưa tìm thấy UserId ''admin'' đang active trong tbl_Users - script sẽ chỉ '
        + N'chuẩn bị sẵn Site/Role/Role_Function tại SiteId = ''' + @SiteId + N''', KHÔNG tạo được '
        + N'dòng tbl_Users (giới hạn mã hoá password, xem ghi chú đầu file) và KHÔNG gán được '
        + N'User_Role. Tạo tài khoản admin trước rồi chạy lại script này.';
END
ELSE
BEGIN
    PRINT N'Tìm thấy UserId ''admin'' tại SiteId = ''' + @SiteId + N'''.';
END

-- ============================================================================
-- 1) Site - tạo nếu chưa có (chỉ áp dụng cho nhánh DB hoàn toàn mới ở trên)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM tbl_Site WHERE SiteId = @SiteId)
BEGIN
    INSERT INTO tbl_Site (SiteId, SiteName, Status, CreatedDate, CreatedBy)
    VALUES (@SiteId, @DefaultSiteName, 1, SYSUTCDATETIME(), @CreatedBy);
    PRINT N'Đã tạo Site ' + @SiteId;
END

-- ============================================================================
-- 2) Role full quyền - tạo nếu chưa có
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM tbl_Roles WHERE RoleId = @RoleId AND SiteId = @SiteId)
BEGIN
    INSERT INTO tbl_Roles (RoleId, RoleName, SiteId, Status, CreatedDate, CreatedBy)
    VALUES (@RoleId, @RoleName, @SiteId, 1, SYSUTCDATETIME(), @CreatedBy);
    PRINT N'Đã tạo Role ' + @RoleId + N' tại Site ' + @SiteId;
END
ELSE IF EXISTS (SELECT 1 FROM tbl_Roles WHERE RoleId = @RoleId AND SiteId = @SiteId AND Status <> 1)
BEGIN
    UPDATE tbl_Roles SET Status = 1, UpdatedDate = SYSUTCDATETIME(), UpdatedBy = @CreatedBy
    WHERE RoleId = @RoleId AND SiteId = @SiteId;
    PRINT N'Đã kích hoạt lại Role ' + @RoleId + N' (trước đó bị soft-delete)';
END

-- ============================================================================
-- 3) Toàn bộ FunctionId đang được [RequireFunction(...)] enforce trong code hiện tại
-- ============================================================================
DECLARE @Functions TABLE (FunctionId VARCHAR(20) PRIMARY KEY);
INSERT INTO @Functions (FunctionId) VALUES
    -- Gateway.Api/Controllers/UserController.cs
    ('CreateUser'), ('UpdateUser'), ('DeleteUser'), ('ImportUsers'),
    -- Site.Api/Controllers/UserRoleController.cs
    ('CreateUserRole'), ('DeleteUserRole'), ('ImportUserRoles'),
    -- Site.Api/Controllers/SiteModuleController.cs
    ('ProvisionSiteModule'), ('ToggleSiteModule'),
    -- Site.Api/Controllers/SiteController.cs
    ('CreateSite'), ('ImportSites'),
    -- Site.Api/Controllers/ScreenController.cs
    ('CreateScreen'), ('UpdateScreen'), ('DeleteScreen'), ('ImportScreens'),
    -- Site.Api/Controllers/RoleFunctionController.cs
    ('CreateRoleFunction'), ('DeleteRoleFunction'), ('ImportRoleFunctions'),
    -- Site.Api/Controllers/RoleController.cs
    ('CreateRole'), ('UpdateRole'), ('DeleteRole'), ('ImportRoles'),
    -- Site.Api/Controllers/PermissionController.cs
    ('AssignRolePermissions'),
    -- Site.Api/Controllers/ModuleController.cs
    ('CreateModule'), ('UpdateModule'), ('DeleteModule'), ('ImportModules'),
    -- Site.Api/Controllers/FunctionController.cs
    ('CreateFunction'), ('UpdateFunction'), ('DeleteFunction'), ('ImportFunctions'),
    -- AdministrativeUnit.Api/Controllers/WardController.cs
    ('CreateWard'), ('UpdateWard'), ('DeleteWard'), ('ImportWards'),
    -- AdministrativeUnit.Api/Controllers/ProvinceController.cs
    ('CreateProvince'), ('UpdateProvince'), ('DeleteProvince'), ('ImportProvinces');
-- Lưu ý: SiteController.UpdateSite/DeleteSite KHÔNG dùng [RequireFunction] (dùng
-- BaseController.IsSiteAdmin(siteId) riêng) - Role full quyền này không ảnh hưởng 2 action đó,
-- IsSiteAdmin chỉ tin theo claim "admin_site" mà Login() tự gán khi user có Role tên "Admin"
-- đúng SiteId tương ứng (RoleName ở đây đã đặt N'Admin' nên vẫn khớp).

-- Reactivate các dòng đã từng bị soft-delete (Status <> 1) thay vì insert trùng - unique index
-- IX_tbl_Role_Function_FunctionId_RoleId không lọc theo Status, insert trùng sẽ lỗi.
UPDATE rf
SET Status = 1, UpdatedDate = SYSUTCDATETIME(), UpdatedBy = @CreatedBy
FROM tbl_Role_Function rf
INNER JOIN @Functions f ON f.FunctionId = rf.FunctionId
WHERE rf.RoleId = @RoleId AND rf.Status <> 1;

-- Cấp mới các Function chưa từng được gán cho Role này
INSERT INTO tbl_Role_Function (RoleId, FunctionId, SiteId, Status, CreatedDate, CreatedBy)
SELECT @RoleId, f.FunctionId, @SiteId, 1, SYSUTCDATETIME(), @CreatedBy
FROM @Functions f
WHERE NOT EXISTS (
    SELECT 1 FROM tbl_Role_Function rf WHERE rf.RoleId = @RoleId AND rf.FunctionId = f.FunctionId
);

-- ============================================================================
-- 4) Gán Role cho User 'admin' (chỉ khi User đã tồn tại - xem bước 0)
-- ============================================================================
IF @UserExists = 1
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tbl_User_Role WHERE UserId = @UserId AND RoleId = @RoleId AND SiteId = @SiteId)
    BEGIN
        INSERT INTO tbl_User_Role (UserId, RoleId, SiteId, Status, CreatedDate, CreatedBy)
        VALUES (@UserId, @RoleId, @SiteId, 1, SYSUTCDATETIME(), @CreatedBy);
        PRINT N'Đã gán Role ' + @RoleId + N' cho User ' + @UserId;
    END
    ELSE IF EXISTS (SELECT 1 FROM tbl_User_Role WHERE UserId = @UserId AND RoleId = @RoleId AND SiteId = @SiteId AND Status <> 1)
    BEGIN
        UPDATE tbl_User_Role SET Status = 1, UpdatedDate = SYSUTCDATETIME(), UpdatedBy = @CreatedBy
        WHERE UserId = @UserId AND RoleId = @RoleId AND SiteId = @SiteId;
        PRINT N'Đã kích hoạt lại User_Role (trước đó bị soft-delete)';
    END

    -- Quyền vừa đổi - xoá cache Redis (key auth:user-permissions:{AutoId}) để có hiệu lực ngay,
    -- không phải đợi TTL 1h hoặc user tự logout/login lại. Script SQL không gọi được Redis, tự xoá
    -- thủ công bằng redis-cli nếu muốn có hiệu lực ngay:
    --   redis-cli DEL auth:user-permissions:<AutoId của tbl_Users.admin ở SiteId trên>
    -- (AutoId lấy từ câu SELECT kiểm tra bên dưới) - hoặc đơn giản là đăng nhập lại.
END

-- ============================================================================
-- 5) Đối chiếu kết quả
-- ============================================================================
SELECT AutoId, UserId, SiteId, UserName FROM tbl_Users WHERE UserId = @UserId;
SELECT RoleId, SiteId, RoleName, Status FROM tbl_Roles WHERE RoleId = @RoleId AND SiteId = @SiteId;
SELECT COUNT(*) AS GrantedFunctionCount FROM tbl_Role_Function WHERE RoleId = @RoleId AND SiteId = @SiteId AND Status = 1;
SELECT * FROM tbl_User_Role WHERE UserId = @UserId AND RoleId = @RoleId AND SiteId = @SiteId;
GO
