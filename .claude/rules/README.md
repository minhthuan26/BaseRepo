# BaseRepo — Coding Rules

Đây là bộ rules bắt buộc tuân thủ khi code trong repo, đúc kết từ [`CLAUDE.md`](../../CLAUDE.md) (nguồn
chuẩn đầy đủ — khi có sai khác, `CLAUDE.md` luôn thắng).

Khi tạo feature mới theo quy trình đầy đủ, dùng skill `create-api-feature` (và các skill con
`create-domain-layer`, `create-application-layer`, `create-infrastructure-layer`,
`create-presentation-layer` trong [`.claude/skills/`](../skills/)). Các file trong thư mục này tập trung
vào **rule xuyên suốt** (cross-cutting) cần nhớ khi viết bất kỳ đoạn code nào.

## Danh sách rule

| File | Nội dung |
|---|---|
| [01-architecture.md](01-architecture.md) | Dependency Rule giữa các layer, Naming Convention, CQRS/MediatR |
| [02-api-response.md](02-api-response.md) | Response chuẩn `Response<T>`, Error Handling (`AppException`, mã lỗi) |
| [03-data-access.md](03-data-access.md) | Data Access (Dapper/Stored Procedure), Dependency Injection |
| [04-security-auth.md](04-security-auth.md) | Authentication/Authorization (JWT, policy), Bảo mật dữ liệu |
| [05-checklist.md](05-checklist.md) | Checklist trước khi thêm feature mới |
| [06-testing.md](06-testing.md) | Quy tắc kiểm thử API: chỉ test riêng API vừa đổi, tài khoản test lấy access token |
