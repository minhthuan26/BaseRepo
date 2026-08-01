# Architecture — Dependency Rule, Naming Convention, CQRS

> Một phần của bộ rules trong [`.claude/rules/`](README.md), đúc kết từ [`CLAUDE.md`](../../CLAUDE.md).

## 1. Kiến trúc — Dependency Rule (bắt buộc)

Chỉ được phụ thuộc **theo một chiều xuống dưới**, không được ngược lại:

```
Presentation → Application, Domain, Infrastructure
Application  → Domain, CommonLibs
Infrastructure → Application (Interfaces), Domain, CommonLibs
Domain       → CommonLibs (chỉ Base classes)
CommonLibs   → Không phụ thuộc service nào
```

- Domain layer **không** được reference Infrastructure hay Application.
- CommonLibs **không** được reference bất kỳ service cụ thể nào (Gateway, Site, ...).

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

Project mới (nếu có) phải theo đúng 4 hậu tố layer: `{Service}`, `{Service}.Application`, `{Service}.Domain`, `{Service}.Infrastructure`.

## 3. CQRS với MediatR

- **Command** (ghi dữ liệu): kế thừa `IRequest<TResponse>`, có property `Data` kiểu `{Name}InputDTO`.
- **Query GetById**: `IRequest<{Name}OutputDTO>`, property `Data` kiểu `string`.
- **Query GetAll**: `IRequest<BaseFilterOutput<{Name}OutputDTO>>`, property `Filter` kiểu `{Name}FilterDTO`.
- Handler luôn inject repository qua interface (`I{Name}Repository`), không gọi thẳng DbContext/SQL trong handler.
- Mỗi Command/Query nằm trong thư mục riêng theo entity: `Features/{EntityName}/{Action}{EntityName}/`.
