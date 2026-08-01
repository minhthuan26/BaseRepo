# Authentication, Authorization & Bảo mật

> Một phần của bộ rules trong [`.claude/rules/`](README.md), đúc kết từ [`CLAUDE.md`](../../CLAUDE.md).

## 1. Authentication & Authorization

- JWT Bearer Token, dùng policy có sẵn: `AdminPolicy`, `UserOrAdminPolicy`.
- Controller action cần policy phải khai báo `[Authorize(Policy = "...")]`, không tự viết logic check role thủ công trong action nếu đã có policy tương ứng.
- Logic phân quyền phức tạp đặt trong `AuthorizationMiddleware`, không nhét vào Controller/Handler.

## 2. Bảo mật & dữ liệu (kế thừa từ global instructions)

- Không hardcode API key/secret/token/mật khẩu — dùng `appsettings.json` + biến môi trường, nhắc nếu thiếu `.env`/config tương ứng.
- Không log dữ liệu nhạy cảm (password, token, PII) ra console/log4net.
- Thận trọng với thay đổi có tính phá huỷ (xoá file, drop table, migration không hồi phục) — luôn hỏi trước khi thực thi.
