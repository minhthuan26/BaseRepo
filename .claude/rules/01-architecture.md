# Architecture — Dependency Rule, Naming Convention, CQRS

> Một phần của bộ rules trong [`.claude/rules/`](README.md), đúc kết từ [`CLAUDE.md`](../../CLAUDE.md).

## 1. Kiến trúc — Dependency Rule (bắt buộc)

Chỉ được phụ thuộc **theo một chiều xuống dưới**, không được ngược lại:

```
Presentation → Application, Contract, Domain, Infrastructure
Application  → Contract (DTOs), Domain (Entities), CommonLibs
Infrastructure → Application (Interfaces), Contract (DTOs), Domain (Entities), CommonLibs
Contract     → CommonLibs (chỉ Base DTOs) — không reference Domain/Application/Infrastructure
Domain       → CommonLibs (chỉ Base classes)
CommonLibs   → Không phụ thuộc service nào
```

- Domain layer **không** được reference Infrastructure hay Application.
- Contract layer **không** được reference Domain/Application/Infrastructure — chỉ CommonLibs.
- CommonLibs **không** được reference bất kỳ service cụ thể nào (Gateway, Site, ...).
- Cả 4 service hiện có (`Gateway`, `Site`, `AdministrativeUnit`, `Booking`) đều đã tách Contract — DTO luôn nằm ở
  `{Service}.Contract/DTOs/`, không còn ở `{Service}.Domain/DTOs/` (xem `claude.md` mục Progress Log 2026-09-01).
  Service mới tạo sau này áp dụng ngay quy ước này từ đầu.

## 2. Naming Convention (bắt buộc theo đúng bảng)

| Loại | Convention | Ví dụ |
|---|---|---|
| Entity | `{Name}.cs` | `User.cs` |
| DTO Input | `{Name}InputDTO` | `UserInputDTO` |
| DTO Output | `{Name}OutputDTO` | `UserOutputDTO` |
| DTO Filter | `{Name}FilterDTO` | `UserFilterDTO` |
| Interface Repository | `I{Name}Repository` | `IUserRepository` |
| Repository | `{Name}Repository` | `UserRepository` |
| Command | `{Action}{Name}Command` | `CreateUserCommand` |
| Command Handler | `{Action}{Name}CommandHandler` | `CreateUserCommandHandler` |
| Query | `{Action}{Name}Query` | `GetAllUsersQuery` |
| Query Handler | `{Action}{Name}QueryHandler` | `GetAllUsersQueryHandler` |
| Controller | `{Name}Controller` | `UserController` |
| Mapping Profile | `{Service}Mapping` | `GatewayMapping` |
| DbContext | `{Service}DbContext` | `GatewayDbContext` |

Project mới phải theo đúng 5 hậu tố layer: `{Service}.Api` (Presentation), `{Service}.Application`,
`{Service}.Contract` (DTO), `{Service}.Domain` (Entity), `{Service}.Infrastructure` — cả 5 gom vào 1 folder cha
`{Service}/`, và `{Service}/` nằm trong 1 `{DomainGroup}/` (`Base/` cho hạ tầng dùng chung, hoặc tên domain
nghiệp vụ — VD `SportsBooking/`) — xem `claude.md` mục "Cấu trúc thư mục vật lý".

## 3. CQRS với MediatR

- **Command** (ghi dữ liệu): kế thừa `IRequest<TResponse>`, có property `Data` kiểu `{Name}InputDTO`.
- **Query GetById**: `IRequest<{Name}OutputDTO>`, property `Data` kiểu `string`.
- **Query GetAll**: `IRequest<BaseFilterOutput<{Name}OutputDTO>>`, property `Filter` kiểu `{Name}FilterDTO`.
- Handler luôn inject repository qua interface (`I{Name}Repository`), không gọi thẳng DbContext/SQL trong handler.
- Mỗi Command/Query nằm trong thư mục riêng theo entity: `Features/{EntityName}/{Action}{EntityName}/`.
