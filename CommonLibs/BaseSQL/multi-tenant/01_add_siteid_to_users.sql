/*
  Bước 1/2 gắn SiteId vào tbl_Users: thêm cột SiteId (nullable) + backfill từ tbl_User_Site.

  Yêu cầu tiên quyết: đã chạy 00_check_multi_site_users.sql và xử lý xong mọi user đang
  active ở nhiều Site cùng lúc (mục 1 của file đó). Nếu chưa xử lý, backfill bên dưới vẫn
  chạy được nhưng sẽ TỰ CHỌN 1 trong các Site đang active cho user đó (theo AutoId nhỏ
  nhất trong tbl_User_Site - coi như Site gán sớm nhất) - không chắc đúng ý nghiệp vụ,
  nên xử lý xong bước rà soát trước khi chạy file này.

  Cột để nullable ở bước này (chưa ép NOT NULL) để có thể chạy nhiều lần / sửa tay từng
  user riêng lẻ (VD user ở mục 2 của 00_check_multi_site_users.sql - chưa có UserSite
  active nào) trước khi khoá cứng ở 02_finalize_users_siteid.sql.
*/

USE BaseRepo;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.tbl_Users') AND name = 'SiteId'
)
BEGIN
    ALTER TABLE [dbo].[tbl_Users] ADD [SiteId] VARCHAR(20) NULL;
END
GO

-- Backfill: mỗi User lấy Site đang active (Status = 1) có AutoId nhỏ nhất trong
-- tbl_User_Site (nhỏ nhất = gán sớm nhất -> ưu tiên Site "gốc" nếu còn sót nhiều dòng
-- active mà bước rà soát 00_check_multi_site_users.sql chưa kịp xử lý).
;WITH PrimarySite AS (
    SELECT
        us.UserId
        ,us.SiteId
        ,ROW_NUMBER() OVER (PARTITION BY us.UserId ORDER BY us.AutoId ASC) AS rn
    FROM tbl_User_Site us
    WHERE us.[Status] = 1
)
UPDATE u
SET u.SiteId = ps.SiteId
FROM tbl_Users u
INNER JOIN PrimarySite ps ON ps.UserId = u.UserId AND ps.rn = 1
WHERE u.SiteId IS NULL;
GO

-- Users KHÔNG backfill được (không có tbl_User_Site active nào) - đối chiếu với mục 2
-- của 00_check_multi_site_users.sql. Gán SiteId thủ công cho từng user ở đây trước khi
-- chạy 02_finalize_users_siteid.sql, ví dụ:
--   UPDATE tbl_Users SET SiteId = 'SITE001' WHERE UserId = 'admin' AND SiteId IS NULL;
SELECT UserId, UserName, [Status]
FROM tbl_Users
WHERE SiteId IS NULL
ORDER BY UserId;
GO
