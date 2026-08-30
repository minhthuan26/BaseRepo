/*
  Dọn dẹp toàn bộ fixture được tạo bởi seed_two_site_isolation.sql (cùng thư mục).

  Xoá theo đúng thứ tự con -> cha (repo KHÔNG dùng FOREIGN KEY - xem BaseSQL.sql và toàn bộ
  migration trong CommonLibs/BaseSQL/multi-tenant/ - nên thứ tự xoá không bị DB ép buộc, nhưng
  vẫn giữ đúng thứ tự con->cha cho rõ ràng và an toàn nếu sau này có ai thêm FK).

  Chỉ xoá đúng các object mang tiền tố ISOTEST_ (Site/Role) hoặc isotest_ (User) và các dòng
  tbl_Role_Function/tbl_User_Role/tbl_Site_Module tham chiếu tới chúng - KHÔNG đụng tới bất kỳ
  dữ liệu thật nào khác. An toàn để chạy nhiều lần (mọi câu lệnh đều tự nhiên no-op nếu dữ liệu
  đã không còn).

  Lưu ý: KHÔNG xoá Module đã "mượn" ở bước 6 của seed script - script đó chỉ mượn 1 Module có
  sẵn để bật/tắt tbl_Site_Module, không tạo Module mới, nên không có gì của tbl_Modules cần dọn.
*/

USE BaseRepo;
GO

-- 1) tbl_Site_Module - entitlement của 2 Site test
DELETE FROM tbl_Site_Module WHERE SiteId IN ('ISOTEST_A', 'ISOTEST_B');
GO

-- 2) tbl_Role_Function - quyền gán cho Role test ở 2 Site
DELETE FROM tbl_Role_Function WHERE RoleId = 'ISOTEST_ROLE' AND SiteId IN ('ISOTEST_A', 'ISOTEST_B');
GO

-- 3) tbl_User_Role - gán Role test cho User test ở 2 Site
DELETE FROM tbl_User_Role WHERE RoleId = 'ISOTEST_ROLE' AND UserId = 'isotest_user' AND SiteId IN ('ISOTEST_A', 'ISOTEST_B');
GO

-- 4) tbl_Roles - Role test ở 2 Site (RoleId trùng nhau, khác SiteId)
DELETE FROM tbl_Roles WHERE RoleId = 'ISOTEST_ROLE' AND SiteId IN ('ISOTEST_A', 'ISOTEST_B');
GO

-- 5) tbl_Users - User test ở 2 Site (UserId trùng nhau, khác SiteId/AutoId)
DELETE FROM tbl_Users WHERE UserId = 'isotest_user' AND SiteId IN ('ISOTEST_A', 'ISOTEST_B');
GO

-- 6) tbl_Site - 2 Site test
DELETE FROM tbl_Site WHERE SiteId IN ('ISOTEST_A', 'ISOTEST_B');
GO

PRINT 'Dọn dẹp hoàn tất. Kiểm tra nhanh (tất cả phải trả về 0 dòng):';
SELECT * FROM tbl_Site WHERE SiteId IN ('ISOTEST_A', 'ISOTEST_B');
SELECT * FROM tbl_Users WHERE UserId = 'isotest_user';
SELECT * FROM tbl_Roles WHERE RoleId = 'ISOTEST_ROLE';
SELECT * FROM tbl_Role_Function WHERE RoleId = 'ISOTEST_ROLE';
SELECT * FROM tbl_User_Role WHERE RoleId = 'ISOTEST_ROLE';
SELECT * FROM tbl_Site_Module WHERE SiteId IN ('ISOTEST_A', 'ISOTEST_B');
GO
