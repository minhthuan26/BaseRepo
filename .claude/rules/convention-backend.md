---
description: "Convention Backend (.NET 10 / ASP.NET Core Web API, Clean Architecture 5-layer + CQRS (MediatR))"
paths:
  - '**/*.cs'
---

# Convention Backend (.NET 10 / ASP.NET Core Web API, Clean Architecture 5-layer + CQRS)

> Bản tóm tắt theo code thật của repo. Chi tiết kiến trúc, naming từng loại class, Response, DI: xem
> `.claude/rules/01-architecture.md` → `05-checklist.md` và `claude.md` ở gốc repo (nguồn chuẩn, thắng khi sai khác).

## C# — đặt tên
| Đối tượng | Quy tắc | Ví dụ đúng (trong repo) | Ví dụ sai |
|---|---|---|---|
| Class, record, enum, method, property | PascalCase | `UserRepository`, `GetFunctionsByRole` | `userRepository`, `get_user` |
| Interface | `I` + PascalCase | `IUserRepository` | `UserRepositoryInterface` |
| Tham số, biến cục bộ | camelCase | `roleId` | `RoleId`, `role_id` |
| Field private | `_camelCase` | `_dbContext`, `_errorPrefix` | `dbContext_` |
| Method repository/interface | `{Action}{Name}`, **không** hậu tố `Async` (theo quy ước có sẵn) | `CreateUser`, `GetAllUsers` | `CreateUserAsync` |
| File | trùng tên class, một class một file (DTO cùng entity được gom chung `{Name}DTO.cs`) | `UserRepository.cs`, `UserDTO.cs` | `Repositories.cs` |
| Namespace | theo project + thư mục | `Site.Application.Features.Role.CreateRole` | — |

## Phân tầng (bắt buộc)
- 5 project/service: `{Service}.Api` → `.Application` → `.Contract` (DTO) / `.Domain` (Entity) → `.Infrastructure`, gom trong `{DomainGroup}/{Service}/`. Dependency chỉ hướng xuống (xem `01-architecture.md`).
- Controller kế thừa `BaseController<T>`, chỉ gọi `IMediator` và trả `Response<T>`; không chứa nghiệp vụ, không gọi repository trực tiếp.
- Handler chỉ gọi `I{Name}Repository`; SQL chỉ nằm trong Repository (`BaseRepository<T>`, 3 region `Stored Procedure` / `Repository Methods` / `Helpers`).
- DTO ở ranh giới API (`{Name}InputDTO`/`OutputDTO`/`FilterDTO` kế thừa base của CommonLibs); không trả Entity ra ngoài.

## Lỗi, log, bảo mật
- Lỗi nghiệp vụ ném `AppException` kèm `HttpStatusCode`, mã lỗi `[{Name}_Repo_{Method}-NN]` (dùng `_errorPrefix`). Không nuốt lỗi; để `GlobalExceptionMiddleware` xử lý.
- Log qua `ILogger<T>` có sẵn; không log password/token/PII (header nhạy cảm đi qua `SensitiveHeaderMasker`).
- Dữ liệu Site-scoped: `SiteId` luôn lấy từ claim `site_id`, không tin input client.
- Action mutating gắn `[RequireFunction("{TênAction}")]`; quyền đọc theo policy (`UserOrAdminPolicy`) hoặc anonymous có chủ đích.
- Không hardcode chuỗi kết nối/khóa; đọc từ `appsettings.json`/biến môi trường.

## Chung
- I/O dùng async/await xuyên suốt, truyền `CancellationToken`; không `.Result`/`.Wait()`.
- Truy vấn Dapper luôn tham số hóa, không nối chuỗi từ input.
- Không để code chết, không `TODO` thiếu mã task. Comment giải thích **vì sao**.
- Thay đổi hành vi nên kèm unit test (xUnit, xem `convention-testing.md`).
