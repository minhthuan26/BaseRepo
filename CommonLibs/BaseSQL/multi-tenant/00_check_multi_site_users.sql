/*
  Pre-flight check: rà soát dữ liệu tbl_User_Site TRƯỚC khi migrate sang mô hình
  "1 User chỉ thuộc đúng 1 Site" (xem tài liệu thiết kế multi-tenant, mục 06 bước 1
  - https://claude.ai/code/artifact/287ffb5c-0fc3-4fd0-81ec-a9f61d4e018b).

  Không có lệnh ghi dữ liệu nào trong file này - chỉ SELECT để biết phạm vi ảnh hưởng.
  Chạy trước khi chạy 01_add_siteid_to_users.sql, an toàn chạy lại nhiều lần.

  Nếu phần 1 ("Users đang active ở nhiều Site") trả về > 0 dòng, PHẢI xử lý thủ công
  trước khi migrate tiếp - thiết kế mới không cho phép 1 User thuộc nhiều Site cùng lúc.
  Hai lựa chọn:
    a) Giữ đúng 1 Site cho user đó - soft-delete (Status = -1) các dòng tbl_User_Site
       còn lại TRƯỚC khi chạy 01_add_siteid_to_users.sql, để backfill tự động lấy đúng
       Site còn active duy nhất.
    b) Tách thành nhiều User riêng biệt (mỗi Site 1 tài khoản đăng nhập khác nhau) - cần
       làm thủ công qua ứng dụng, ngoài phạm vi các script trong thư mục này.
*/

USE BaseRepo;
GO

-- 1) Users đang active (Status = 1) ở nhiều hơn 1 Site cùng lúc - cần xử lý trước khi
--    migrate tiếp, xem hướng dẫn ở đầu file.
SELECT
    us.UserId
    ,COUNT(*) AS ActiveSiteCount
    ,STRING_AGG(us.SiteId, ', ') AS SiteIds
FROM tbl_User_Site us
WHERE us.[Status] = 1
GROUP BY us.UserId
HAVING COUNT(*) > 1
ORDER BY ActiveSiteCount DESC;
GO

-- 2) Users chưa từng có dòng tbl_User_Site nào active (kể cả đã soft-delete) - backfill
--    tự động ở 01_add_siteid_to_users.sql sẽ KHÔNG gán được SiteId cho những user này,
--    cần gán tay trước khi chạy 02_finalize_users_siteid.sql (xem SELECT cuối file 01).
SELECT u.UserId, u.UserName, u.[Status]
FROM tbl_Users u
WHERE NOT EXISTS (
    SELECT 1 FROM tbl_User_Site us WHERE us.UserId = u.UserId AND us.[Status] = 1
)
ORDER BY u.UserId;
GO

-- 3) Tổng quan số lượng - lưu lại kết quả này để đối chiếu sau khi migrate xong
--    (TotalUsers nên bằng tổng số dòng tbl_Users có SiteId sau khi hoàn tất 01+02).
SELECT
    (SELECT COUNT(*) FROM tbl_Users) AS TotalUsers
    ,(SELECT COUNT(DISTINCT UserId) FROM tbl_User_Site WHERE [Status] = 1) AS UsersWithActiveSite
    ,(SELECT COUNT(*) FROM tbl_Site) AS TotalSites
    ,(SELECT COUNT(*) FROM tbl_Modules) AS TotalModules;
GO
