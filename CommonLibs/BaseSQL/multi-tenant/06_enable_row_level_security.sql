/*
  Row-Level Security theo SiteId - lưới an toàn thứ hai (xem tài liệu thiết kế, mục 04).

  TUỲ CHỌN - chỉ bật sau khi:
    1. Toàn bộ code C# (AuditContext, BaseRepository, mọi nơi mở connection...) đã set
       đúng SESSION_CONTEXT('SiteId') TRƯỚC khi query - bật RLS quá sớm sẽ chặn nhầm
       chính request hợp lệ nếu code chưa kịp set context.
    2. Đã test kỹ ở môi trường staging - RLS áp dụng cho MỌI câu query lên 4 bảng bên
       dưới, kể cả khi bạn tự debug bằng SSMS (trừ khi đang là thành viên db_owner, xem
       predicate function bên dưới).

  Ở bản deploy on-prem (chỉ 1 Site), RLS không bắt buộc nhưng bật sẵn để hành vi nhất
  quán giữa 2 hình thức bán hàng (thuê trọn bộ / on-prem).
*/

USE BaseRepo;
GO

IF OBJECT_ID('dbo.fn_SitePredicate', 'IF') IS NOT NULL
BEGIN
    -- Không thể DROP FUNCTION trong khi còn gắn với security policy đang bật - tắt/drop
    -- policy trước (khối bên dưới), rồi mới tới đây được. Giữ khối này để script chạy lại
    -- được an toàn (idempotent) nếu cần chỉnh sửa predicate sau này.
    DROP FUNCTION [dbo].[fn_SitePredicate];
END
GO

CREATE FUNCTION [dbo].[fn_SitePredicate](@SiteId VARCHAR(20))
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_result
    WHERE @SiteId = CAST(SESSION_CONTEXT(N'SiteId') AS VARCHAR(20))
       OR IS_MEMBER('db_owner') = 1;   -- cho phép DBA/tài khoản migration tự bypass khi cần
GO

IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'SiteIsolationPolicy')
BEGIN
    ALTER SECURITY POLICY [SiteIsolationPolicy] WITH (STATE = OFF);
    DROP SECURITY POLICY [SiteIsolationPolicy];
END
GO

CREATE SECURITY POLICY [SiteIsolationPolicy]
    ADD FILTER PREDICATE [dbo].[fn_SitePredicate]([SiteId]) ON [dbo].[tbl_Users]
    ,ADD FILTER PREDICATE [dbo].[fn_SitePredicate]([SiteId]) ON [dbo].[tbl_Roles]
    ,ADD FILTER PREDICATE [dbo].[fn_SitePredicate]([SiteId]) ON [dbo].[tbl_Role_Function]
    ,ADD FILTER PREDICATE [dbo].[fn_SitePredicate]([SiteId]) ON [dbo].[tbl_User_Role]
    WITH (STATE = ON);
GO

-- Nhắc khi wire vào code (CommonLibs/BaseDbContext/BaseDbContext.cs hoặc nơi mở
-- connection): mỗi connection mới phải chạy trước mọi câu SELECT/UPDATE khác:
--   EXEC sp_set_session_context @key = N'SiteId', @value = @siteId, @read_only = 1;
-- Chưa set thì SESSION_CONTEXT('SiteId') trả về NULL -> predicate luôn false (trừ
-- db_owner) -> mọi query lên 4 bảng trên trả về rỗng, không phải lỗi, dễ nhầm là bug.

-- Muốn tắt tạm để debug mà không xoá hẳn policy:
--   ALTER SECURITY POLICY [SiteIsolationPolicy] WITH (STATE = OFF);
-- Bật lại:
--   ALTER SECURITY POLICY [SiteIsolationPolicy] WITH (STATE = ON);
