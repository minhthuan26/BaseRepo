# Testing — Quy tắc khi kiểm thử API

> Một phần của bộ rules trong [`.claude/rules/`](README.md), đúc kết từ yêu cầu của người dùng.

## 1. Phạm vi test khi thêm/sửa 1 API

- Khi vừa hoàn thành hoặc sửa **1 API cụ thể**, hãy hỏi xác nhận và khi được xác nhận test thì chỉ test riêng API đó (happy path + edge case liên quan
  trực tiếp).
- Không tự ý chạy lại toàn bộ test suite / test tất cả API khác trong service trừ khi được yêu cầu rõ, hoặc
  thay đổi đó chắc chắn ảnh hưởng tới API khác (VD: sửa `BaseRepository`, sửa middleware dùng chung).

## 2. Lấy access token khi test API cần Authorize

Với các API có `[Authorize(Policy = "...")]` (VD: `AdminPolicy`, `UserOrAdminPolicy`), trước khi gọi API
cần test, gửi request đăng nhập để lấy access token:

- Endpoint: Login (`AuthorizationController`, xem `Gateway.Application/Features/Authorization/Login`).
- Tài khoản test dùng chung cho việc kiểm thử API cục bộ:
  - `userId`: `admin`
  - `password`: `admin2604`
- Gắn `access_token` nhận được vào header `Authorization: Bearer {token}` khi gọi API đang test.

> Lưu ý: đây là tài khoản test dùng cho môi trường local/dev, không dùng để test trên môi trường
> production hay commit thêm bất kỳ tài khoản/secret nào khác vào rules.

## 3. Checklist test cách ly 2 Site (multi-tenant)

Bất kỳ thay đổi nào chạm tới `tbl_Users`, `tbl_Roles`, `tbl_Role_Function`, `tbl_User_Role`,
`tbl_Site_Module`, hoặc bất kỳ Repository/Controller nào lọc dữ liệu theo `SiteId`, phải chạy
lại bộ test cách ly 2 Site trước khi coi là xong — mục tiêu là chứng minh **không có API nào
trả/ghi chéo dữ liệu giữa 2 Site**, kể cả khi 2 Site dùng trùng `UserId`/`RoleId`.

Xem `CommonLibs/BaseSQL/multi-tenant/README.md` để biết bối cảnh đầy đủ (đây là hạng mục đã
được đánh dấu "Chưa làm" trong mục "Không nằm trong các script này" của README đó cho tới khi
bộ test này được viết).

- [ ] Đã chạy đủ migration `00`-`08` trong `CommonLibs/BaseSQL/multi-tenant/` theo đúng thứ tự
      trong README — đặc biệt `08_fix_roleid_unique_scope.sql` (bắt buộc để `RoleId` được phép
      trùng giữa 2 Site, tiền đề của toàn bộ test này).
- [ ] Chạy `CommonLibs/BaseSQL/multi-tenant/tests/seed_two_site_isolation.sql` để tạo fixture:
      2 Site test (`ISOTEST_A`, `ISOTEST_B`), 1 User trùng `UserId` (`isotest_user`) và 1 Role
      trùng `RoleId` (`ISOTEST_ROLE`) độc lập ở mỗi Site, quyền Function cấp lệch giữa 2 Site.
- [ ] Đảm bảo Gateway đang chạy (`http://localhost:80` mặc định) và Site service đang chạy ở
      cổng nội bộ (`http://localhost:8001` mặc định) — 2 action nội bộ của `SiteModuleController`
      không có route YARP nên script test gọi thẳng cổng Site.
- [ ] Chạy `CommonLibs/BaseSQL/multi-tenant/tests/test_two_site_isolation.sh` (cần `curl`, `jq`,
      `base64`) — script tự in `PASS`/`FAIL`/`SKIP` cho từng bước và thoát mã khác 0 nếu có
      `FAIL`. Các nhóm test chính:
  - Login cùng `UserId` ở 2 Site trả về 2 token khác nhau (`auto_id`/`site_id` claim khác nhau).
  - `GetRolesByUser`, `GetRole`, `GetAllRoleFunctions`, `GetAllUserRoles`, `GetAllUsers` chỉ trả
    đúng dữ liệu của Site trong token, dù `UserId`/`RoleId` trùng chuỗi với Site kia.
  - `PermissionController.GetPermissionTree` bỏ qua `siteId` truyền trên route, luôn dùng claim.
  - `CreateUserRole` từ chối gán Role không thuộc Site của người gọi dù đúng `RoleId` literal.
  - `[RequireFunction]` (`SiteModuleController.ToggleSiteModule`) chặn đúng theo Site cấp quyền,
    không bị "ăn theo" chỉ vì Role trùng tên với Site có quyền.
- [ ] Nếu có `FAIL`: **không được** coi thay đổi vừa làm là an toàn để merge/deploy — sửa xong
      rồi chạy lại toàn bộ script, không chỉ riêng lệnh vừa fail (một fix sai chỗ khác có thể
      vừa vá lại vừa mở thêm 1 lỗ hổng chéo Site khác).
- [ ] Dọn fixture sau khi test xong bằng
      `CommonLibs/BaseSQL/multi-tenant/tests/cleanup_two_site_isolation.sql` — không để lại
      Site/User/Role test (tiền tố `ISOTEST_`/`isotest_`) trong DB dùng chung với dữ liệu thật.

---

*File này chỉ tóm tắt quy trình test nội bộ — nguồn chuẩn kiến trúc/coding đầy đủ vẫn là
[`CLAUDE.md`](../../CLAUDE.md).*
