# Checklist trước khi thêm feature mới

> Một phần của bộ rules trong [`.claude/rules/`](README.md), đúc kết từ [`CLAUDE.md`](../../CLAUDE.md).

1. Đọc code tương tự đã có trong cùng service để bám naming/pattern (không tự sáng tạo cách làm mới).
2. Domain Layer: Entity trong `{Service}.Domain/Entities/`.
3. Contract Layer: DTO (Input/Output/Filter) trong `{Service}.Contract/DTOs/`.
4. Application Layer: Interface Repository → Feature (Command/Query + Handler) → Mapping profile.
5. Infrastructure Layer: Implement Repository (Dapper/SP) → đăng ký DI.
6. Presentation Layer: Controller dùng `IMediator`, trả `Response<T>`.
7. Không refactor/dọn code ngoài phạm vi task được giao.

> Khi cần quy trình chi tiết từng bước theo layer, dùng skill `create-api-feature` (và các skill con
> `create-domain-layer`, `create-contract-layer`, `create-application-layer`, `create-infrastructure-layer`,
> `create-presentation-layer` trong `.claude/skills/`).
