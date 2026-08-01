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

---

*File này chỉ tóm tắt quy trình test nội bộ — nguồn chuẩn kiến trúc/coding đầy đủ vẫn là
[`CLAUDE.md`](../../CLAUDE.md).*
