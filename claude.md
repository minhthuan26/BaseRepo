# BaseRepo - Developer Guide

## 🏗️ Tổng quan kiến trúc

Project **BaseRepo** là một hệ thống Microservices backend sử dụng **.NET 6**, được xây dựng theo kiến trúc **Clean Architecture** kết hợp **CQRS (Command Query Responsibility Segregation)** thông qua thư viện **MediatR**.

### Công nghệ chính

| Thành phần | Công nghệ |
|---|---|
| Framework | .NET 6, ASP.NET Core Web API |
| Architecture | Clean Architecture + CQRS |
| Mediator | MediatR |
| ORM / Data Access | Entity Framework Core, Dapper |
| Mapping | AutoMapper |
| Database | SQL Server (Stored Procedures + Raw SQL) |
| API Gateway | YARP (Yet Another Reverse Proxy) |
| Authentication | JWT Bearer Token |
| Logging | log4net, Dynamic Logging |
| Service Discovery | Consul |
| Message Queue | RabbitMQ |
| Caching | Redis |
| Search Engine | Elasticsearch + Filebeat + Kibana |
| Encryption | EncryptionLibrary (custom DLL) |

---

## 📁 Cấu trúc thư mục tổng quan

```
BaseRepo/
├── BaseRepo.slnx                    # Solution file
│
├── CommonLibs/                       # 🔧 Shared Library (dùng chung cho tất cả services)
│   ├── BaseDbContext/                # Base DbContext với Dapper extensions
│   │   └── BaseDbContext.cs
│   ├── BaseRabbitMQ/                 # RabbitMQ base consumer
│   ├── BaseSQL/                      # SQL helpers
│   ├── Consul/                       # Consul service discovery
│   ├── Controllers/                  # Base controller
│   │   └── BaseController.cs
│   ├── DTOs/                         # Base DTOs (Input, Output, Filter)
│   │   ├── BaseDTO.cs                # BaseOutputDTO, BaseInputDTO, BaseFilterOutput<T>
│   │   └── BaseFilterDTO.cs          # BaseFilterDTO (phân trang, sắp xếp)
│   ├── Entities/                     # Base entity
│   │   └── BaseEntity.cs             # AutoId, Status, Audit fields
│   ├── Enums/                        # Enum dùng chung
│   ├── Icons/                        # Icon resources
│   ├── Libs/                         # External libraries (DLL)
│   │   └── EncryptionLibrary.dll
│   ├── Logging/                      # Dynamic logging configuration
│   ├── Mappings/                     # Base AutoMapper profiles
│   ├── Middlewares/                  # Global exception middleware
│   │   └── GlobalExceptionMiddleware.cs
│   ├── Options/                      # App configuration options
│   │   ├── AppOptions.cs
│   │   └── ServiceDiscoveryInfo.cs
│   ├── Repositories/                 # Base repository (ExecuteProcAsync)
│   │   └── BaseRepository.cs
│   ├── Services/                     # Shared services
│   │   ├── JwtTokenService.cs
│   │   ├── RabbitMQService.cs
│   │   ├── RedisService.cs
│   │   ├── ServiceCollectionExtensions.cs  # AddCommonServices<TDbContext>()
│   │   ├── EnsureServiceRun.cs
│   │   └── ...
│   └── Utils/                        # Utility classes
│       ├── AppException.cs           # Custom exception với HttpStatusCode
│       ├── AuditContext.cs           # IAuditContext (UserName, DeviceId, DeviceName)
│       ├── Response.cs               # Response<T> wrapper chuẩn
│       ├── FileHelper.cs
│       ├── Validator.cs
│       └── ...
│
├── Gateway/                          # 🌐 API Gateway (Presentation Layer)
│   ├── Controllers/
│   │   ├── AuthorizationController.cs
│   │   ├── UserController.cs
│   │   └── TestGatewayAPIController.cs
│   ├── Utils/
│   ├── Keys/                         # JWT signing keys
│   ├── Program.cs                    # Entry point + DI + YARP + Auth config
│   ├── appsettings.json              # Config (YARP routes, DB, JWT, Consul...)
│   └── log4net.config
│
├── Gateway.Application/              # 📋 Application Layer (CQRS Handlers)
│   ├── Features/                     # Feature-based organization
│   │   ├── Authorization/
│   │   │   ├── Login/
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   └── LoginCommandHandler.cs
│   │   │   └── Logout/
│   │   │       ├── LogoutCommand.cs
│   │   │       └── LogoutCommandHandler.cs
│   │   └── User/
│   │       ├── CreateUser/
│   │       │   ├── CreateUserCommand.cs
│   │       │   └── CreateUserCommandHandler.cs
│   │       ├── GetAllUsers/
│   │       │   ├── GetAllUsersQuery.cs
│   │       │   └── GetAllUsersQueryHandler.cs
│   │       └── GetUserById/
│   │           ├── GetUserByIdQuery.cs
│   │           └── GetUserByIdQueryHandler.cs
│   ├── Interfaces/                   # Repository interfaces
│   │   ├── IAuthorizationRepository.cs
│   │   └── IUserRepository.cs
│   ├── Mappings/                     # AutoMapper profiles
│   │   └── GatewayMapping.cs
│   └── Middlewares/
│       └── AuthorizationMiddleware.cs
│
├── Gateway.Domain/                   # 🏛️ Domain Layer (Entities + DTOs)
│   ├── DTOs/
│   │   ├── AuthorizationDTO.cs
│   │   └── UserDTO.cs               # UserInputDTO, UserOutputDTO, UserFilterDTO
│   └── Entities/
│       └── User.cs                   # Rich Domain Entity với guard methods
│
├── Gateway.Infrastructure/           # 🔌 Infrastructure Layer (Data Access)
│   ├── DBContext/
│   │   └── GatewayDbContext.cs       # DbContext kế thừa BaseContext
│   ├── Repositories/
│   │   ├── AuthorizationRepository.cs
│   │   └── UserRepository.cs        # Implement IUserRepository
│   └── DependencyInjection.cs       # Extension method đăng ký DI
│
├── Site/                             # 🏥 Site Microservice (Presentation Layer)
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
│
├── Site.Application/                 # Site Application Layer
│   ├── Features/                     # (cấu trúc tương tự Gateway.Application)
│   ├── Interfaces/
│   │   ├── ISiteRepository.cs
│   │   ├── IModuleRepository.cs
│   │   ├── IRoleRepository.cs
│   │   ├── IScreenRepository.cs
│   │   ├── IFunctionRepository.cs
│   │   └── ...
│   └── Mappings/
│       └── SiteMapping.cs
│
├── Site.Domain/                      # Site Domain Layer
│   ├── DTOs/
│   │   ├── SiteDTO.cs
│   │   ├── ModuleDTO.cs
│   │   ├── RoleDTO.cs
│   │   ├── ScreenDTO.cs
│   │   ├── FunctionDTO.cs
│   │   └── ...
│   └── Entities/
│       ├── Site.cs
│       ├── Module.cs
│       ├── Role.cs
│       ├── Screen.cs
│       ├── Function.cs
│       └── ...
│
├── Site.Infrastructure/              # Site Infrastructure Layer
│   ├── DBContext/
│   │   └── SiteDbContext.cs
│   ├── Repositories/
│   │   └── SiteRepository.cs
│   └── DependencyInjection.cs
│
├── Elastic/                          # 📊 Elasticsearch + Kibana + Filebeat tools
│
└── SystemManagement/                 # 🖥️ Windows desktop management tool (WinForms)
    ├── Config/
    ├── Interfaces/
    ├── Models/
    └── Views/
```

---

## 🧱 Kiến trúc 4 Layer (cho mỗi Microservice)

```
┌─────────────────────────────────────────────────────┐
│                 Presentation Layer                    │
│            (Gateway / Site / ...)                     │
│         Controllers, Program.cs, Config              │
├─────────────────────────────────────────────────────┤
│                 Application Layer                     │
│         (Gateway.Application / Site.Application)      │
│    Features/ (Commands + Queries + Handlers)          │
│    Interfaces/ (Repository Contracts)                 │
│    Mappings/ (AutoMapper Profiles)                    │
├─────────────────────────────────────────────────────┤
│                  Domain Layer                         │
│         (Gateway.Domain / Site.Domain)                │
│    Entities/ (Rich Domain Models)                     │
│    DTOs/ (InputDTO, OutputDTO, FilterDTO)             │
├─────────────────────────────────────────────────────┤
│               Infrastructure Layer                    │
│   (Gateway.Infrastructure / Site.Infrastructure)      │
│    DBContext/ (EF Core DbContext)                     │
│    Repositories/ (Data Access Implementation)         │
│    DependencyInjection.cs (DI Registration)           │
└─────────────────────────────────────────────────────┘
              ↕ Dependency: Chỉ hướng xuống ↕
```

### Dependency Rule
- **Presentation** → Application, Domain, Infrastructure
- **Application** → Domain, CommonLibs
- **Infrastructure** → Application (Interfaces), Domain, CommonLibs
- **Domain** → CommonLibs (chỉ Base classes)
- **CommonLibs** → Không phụ thuộc vào service nào

---

## 📏 Quy tắc đặt tên (Naming Conventions)

### Projects
```
{ServiceName}                    → Presentation Layer  (VD: Gateway, Site)
{ServiceName}.Application       → Application Layer
{ServiceName}.Domain             → Domain Layer
{ServiceName}.Infrastructure     → Infrastructure Layer
```

### Files & Classes

| Loại | Convention | Ví dụ |
|---|---|---|
| **Entity** | `{Name}.cs` | `User.cs`, `Site.cs` |
| **DTO Input** | `{Name}InputDTO` | `UserInputDTO` |
| **DTO Output** | `{Name}OutputDTO` | `UserOutputDTO` |
| **DTO Filter** | `{Name}FilterDTO` | `UserFilterDTO` |
| **Interface Repository** | `I{Name}Repository` | `IUserRepository` |
| **Repository** | `{Name}Repository` | `UserRepository` |
| **Command** | `{Action}{Name}Command` | `CreateUserCommand` |
| **Command Handler** | `{Action}{Name}CommandHandler` | `CreateUserCommandHandler` |
| **Query** | `{Action}{Name}Query` | `GetAllUsersQuery` |
| **Query Handler** | `{Action}{Name}QueryHandler` | `GetAllUsersQueryHandler` |
| **Controller** | `{Name}Controller` | `UserController` |
| **Mapping Profile** | `{Service}Mapping` | `GatewayMapping` |
| **DbContext** | `{Service}DbContext` | `GatewayDbContext` |

### Thư mục Features (CQRS)
```
Features/
└── {EntityName}/                    # VD: User
    ├── Create{EntityName}/          # Command
    │   ├── Create{EntityName}Command.cs
    │   └── Create{EntityName}CommandHandler.cs
    ├── GetAll{EntityName}s/         # Query (danh sách)
    │   ├── GetAll{EntityName}sQuery.cs
    │   └── GetAll{EntityName}sQueryHandler.cs
    └── Get{EntityName}ById/         # Query (chi tiết)
        ├── Get{EntityName}ByIdQuery.cs
        └── Get{EntityName}ByIdQueryHandler.cs
```

---

## 🔄 CQRS Pattern với MediatR

### Command (Ghi dữ liệu)
- Kế thừa `IRequest<TResponse>` từ MediatR
- Chứa property `Data` kiểu `{Name}InputDTO`
- Handler kế thừa `IRequestHandler<TCommand, TResponse>`
- Handler inject `I{Name}Repository` qua constructor

### Query (Đọc dữ liệu)
- **GetById**: `IRequest<{Name}OutputDTO>`, property `Data` kiểu `string`
- **GetAll**: `IRequest<BaseFilterOutput<{Name}OutputDTO>>`, property `Filter` kiểu `{Name}FilterDTO`

---

## 📦 Base Classes (CommonLibs)

### BaseEntity
```csharp
// Các field chuẩn cho mọi Entity
public long AutoId { get; set; }
public DateTime? UpdatedDate { get; set; }
public DateTime? CreatedDate { get; set; }
public string? CreatedBy { get; set; }
public string? UpdatedBy { get; set; }
public EntityStatus Status { get; set; } = EntityStatus.Active;
public string? DeviceName { get; set; }
public string? DeviceId { get; set; }
```

### BaseInputDTO / BaseOutputDTO
- `BaseInputDTO`: Audit fields + `HeaderMappings` cho import + `SetAuditInfo()`
- `BaseOutputDTO`: Audit fields dạng output (Status là `string`)
- `BaseFilterDTO`: `Status`, `PageNumber`, `PageSize`, `FromDate`, `ToDate`, `SortBy`
- `BaseFilterOutput<T>`: `TotalRecords`, `TotalPages`, `PageNumber`, `PageSize`, `Records`

### BaseRepository<T>
- Inject: `IMapper`, `ILogger<T>`, `IConfiguration`, `IHttpContextAccessor`, `AppOptions`, `IAuditContext`, `IJwtTokenService`
- Cung cấp `ExecuteProcAsync()` cho Stored Procedures

### BaseController<T>
- Inject: `ILogger<T>`, `IMediator`, `IHttpContextAccessor`
- Kế thừa `ControllerBase` với `[ApiController]`

---

## 🌐 API Response chuẩn

Tất cả API trả về dạng `Response<T>`:

```csharp
return Ok(new Response<UserOutputDTO>()
{
    Success = true,
    Code = System.Net.HttpStatusCode.OK,
    Message = "Successfully",
    Data = result
});
```

---

## 🚨 Error Handling

- Sử dụng `AppException` với mã lỗi prefix: `[{Service}_Repo_{Method}-{Number}]`
- VD: `[User_Repo_CreateUser-01] Input is null.`
- `GlobalExceptionMiddleware` bắt exception toàn cục

---

## 🔐 Authentication & Authorization

- JWT Bearer Token
- Policies: `AdminPolicy` (Admin), `UserOrAdminPolicy` (User + Admin)
- Custom `AuthorizationMiddleware` cho logic phân quyền phức tạp
- Controller attribute: `[Authorize(Policy = "UserOrAdminPolicy")]`

---

## 🔌 Dependency Injection

Mỗi service có file `DependencyInjection.cs` trong Infrastructure layer:

```csharp
public static IServiceCollection Add{Service}Infrastructure(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    // 1. Đăng ký EnsureServiceRunning
    services.AddEnsureServiceRunning();
    
    // 2. Đăng ký CommonServices (DbContext, AutoMapper, MediatR...)
    services.AddCommonServices<{Service}DbContext>(configuration, "{Service}.Application");
    
    // 3. Đăng ký Repository nội bộ
    services.AddScoped<I{Name}Repository, {Name}Repository>();
    
    return services;
}
```

Gọi trong `Program.cs`:
```csharp
builder.Services.Add{Service}Infrastructure(builder.Configuration);
```

---

## 📊 Data Access Pattern

- **Dapper** cho raw SQL queries: `_dbContext.QueryAsync<T>()`, `_dbContext.ExecuteAsync()`
- **Stored Procedures**: `ExecuteProcAsync()` từ `BaseRepository`
- **Transactions**: `_dbContext.ExecuteWithTransactionAsync()`
- **Bulk Insert**: `_dbContext.ExecuteBulkAsync()`

### Repository Structure
```csharp
public class {Name}Repository : BaseRepository<{Name}Repository>, I{Name}Repository
{
    private readonly {Service}DbContext _dbContext;
    private readonly string _errorPrefix = "{Name}_Repo_";

    #region Stored Procedure
    #endregion

    #region Repository Methods
    // CRUD operations
    #endregion

    #region Helpers
    // Private helper methods
    #endregion
}
```

---

## 🚀 Thêm Feature mới (Checklist)

1. **Domain Layer**: Tạo Entity, DTOs (Input, Output, Filter)
2. **Application Layer**: 
   - Tạo Interface Repository trong `Interfaces/`
   - Tạo Feature folder với Command/Query + Handler
   - Thêm mapping trong `{Service}Mapping.cs`
3. **Infrastructure Layer**:
   - Implement Repository
   - Đăng ký DI trong `DependencyInjection.cs`
4. **Presentation Layer**:
   - Tạo Controller sử dụng MediatR

---

## 📌 Tiến độ công việc (Progress Log)

> Section này ghi lại các công việc đã làm và việc còn tồn đọng, để phiên làm việc sau đọc và tiếp tục thay vì
> hỏi lại từ đầu. Cập nhật section này sau khi hoàn thành công việc đáng kể — thêm mục mới lên đầu phần
> "Đã hoàn thành", dọn/cập nhật lại phần "Việc tiếp theo" cho khớp thực tế.

*Cập nhật gần nhất: 2026-08-02*

### Đã hoàn thành

- **Thêm API refresh token vào `AuthorizationController`** — `POST /api/authorization/renew-token`. Phần logic
  khó nhất (verify access token đã hết hạn, verify refresh token còn hợp lệ trong `tbl_Users.RefreshToken`,
  generate access+refresh token mới, update DB) **đã có sẵn từ trước** ở `IAuthorizationRepository.RenewToken()`
  / `AuthorizationRepository.RenewToken()` — chỉ thiếu Application layer + Presentation layer để expose ra
  ngoài, nên chỉ cần "nối dây" theo đúng CQRS checklist:
  - Mới: `Gateway.Application/Features/Authorization/RenewToken/{RenewTokenCommand,RenewTokenCommandHandler}.cs`
    (không có property `Data` vì `RenewToken()` không nhận input DTO, tự đọc access token từ header + refresh
    token từ cookie bên trong, giống pattern `LogoutCommand`).
  - `Gateway/Controllers/AuthorizationController.cs` — thêm action `RenewToken()`, **không** gắn
    `[Authorize(...)]` (giống `Login`/`Logout`, vì access token gửi lên có thể đã hết hạn nên không thể gate
    bằng Policy chuẩn).
  - **Refactor nhỏ trong phạm vi task** (đã hỏi và được xác nhận): `AuthorizationRepository.RenewToken()` trước
    đó tự set Cookie `RefreshToken` thẳng trong Infrastructure layer (dùng `_httpContext.HttpContext.Response
    .Cookies.Append`, vi phạm Clean Architecture — Infrastructure không nên đụng HTTP response, khác pattern
    `Login` set cookie ở Controller) — đã bỏ đoạn set cookie đó ra khỏi Repository, chuyển sang set ở
    `AuthorizationController.RenewToken()` sau khi nhận kết quả từ mediator, nhất quán với `Login`.
  - Không cần sửa `IAuthorizationRepository` (đã có sẵn method), DTO, DI, `appsettings.json` (Controller nằm
    trực tiếp ở Gateway, không proxy qua YARP).
  - **Lưu ý hành vi có sẵn (không đổi)**: API yêu cầu access token gửi lên (header `Authorization`) phải **đã
    hết hạn** mới cho renew — nếu còn hạn sẽ bị từ chối với lỗi `[Authorization_Repo_RenewToken-08] Access token
    is not expired.`. Chưa test lại end-to-end (người dùng chủ động test sau).
- **Fix 502 khi Gateway forward header chứa ký tự Unicode sang Site (lộ ra sau khi fix `UserOrAdminPolicy` bên
  dưới)** — test lại `GET /api/permission/{siteId}?roleId=...` bằng token Role `"Ký thuật viên"` sau khi
  `UserOrAdminPolicy` đã cho qua thì gặp lỗi mới: **502 Bad Gateway**. Debug qua `Gateway/Logs` thấy chính xác
  exception `System.Net.Http.HttpRequestException: Request headers must contain only ASCII characters` tại
  `Yarp.ReverseProxy.Forwarder.HttpForwarder.SendAsync`.
  - **Root cause**: `CommonLibs/Middlewares/AuthorizationMiddleware.cs` (dòng set header `X-User-Role`) gán
    thẳng RoleName gốc từ DB (có thể chứa Unicode có dấu, VD `"Ký thuật viên"`) vào HTTP header, rồi YARP
    forward header đó sang Site qua `HttpClient` — `HttpClient` bắt buộc header value chỉ ASCII nên ném
    exception, YARP trả 502. Đây là bug có sẵn từ trước, chỉ bị che khuất vì trước đó `UserOrAdminPolicy` (bug
    ở mục dưới) đã chặn 403 các request loại này từ sớm hơn, chưa từng chạm tới bước forward.
  - **Fix**: bọc `Uri.EscapeDataString(...)` cho cả `X-User-Role` và `X-User-Functions` trước khi gán vào
    header (percent-encode ASCII-safe, decode lại được bằng `Uri.UnescapeDataString` nếu sau này có chỗ đọc).
    Xác nhận qua `grep` là 2 header này **hiện không được đọc ở bất kỳ đâu khác** trong code (chỉ set, không
    consume), nên fix không đổi hành vi nghiệp vụ nào, chỉ đảm bảo request không crash khi forward.
  - Build solution: 0 Error. Chưa test lại end-to-end (người dùng chủ động test sau, cần restart cả Gateway và
    Site).
- **Fix `UserOrAdminPolicy` chặn nhầm user có Role tuỳ ý (không phải literal "User"/"Admin")** — phát hiện qua
  bug thực tế: gọi `GET /api/permission/{siteId}?roleId=...` bằng token của user `ktv_1` (Role claim
  `"Ký thuật viên"`, RoleName thật của Role "ktv") thì nhận **response rỗng, không đúng chuẩn `Response<T>`**
  (`content-length: 0`, không có JSON). Debug bằng log (`Gateway/Logs`) xác nhận request bị **chặn 403 ngay tại
  Gateway** (route `permission-route`), **chưa từng tới được** `PermissionController` bên Site (không có log
  tương ứng ở `Site/Logs`) — do đó không đi qua `Response<T>` wrapper hay `GlobalExceptionMiddleware`, chỉ là
  response 403 mặc định rỗng của ASP.NET Core khi `[Authorize(Policy=...)]` fail.
  - **Root cause**: `UserOrAdminPolicy = policy.RequireRole("User", "Admin")` so khớp **literal** giá trị claim
    Role. Từ khi RBAC đọc đúng `RoleName` thật làm claim Role (xem mục "Kết nối RBAC..." bên dưới), bất kỳ Role
    nào có tên khác `"User"`/`"Admin"` (VD `"Ký thuật viên"`) sẽ luôn fail policy này ngay từ tầng Gateway/Site
    `[Authorize]`, **trước khi** `RequireFunction` hay bất kỳ logic nghiệp vụ nào kịp chạy.
  - **Fix**: đổi định nghĩa `UserOrAdminPolicy` (`Gateway/Program.cs`, `Site/Program.cs`) từ `RequireRole` sang
    `RequireAssertion` — chỉ cần user **đã đăng nhập và có ít nhất 1 Role claim** (không quan tâm RoleName cụ
    thể là gì), đúng tinh thần đã ghi trong mục RBAC bên dưới ("Class-level Authorize... bắt buộc phải login +
    có ít nhất 1 Role", việc chặn cụ thể theo hành động do `RequireFunction`/`AdminPolicy` đảm nhiệm).
    `AdminPolicy` giữ nguyên `RequireRole("Admin")` (literal, không đổi).
  - Build solution: 0 Error (chỉ warning nullable có sẵn từ trước, không liên quan).
  - **Chưa test lại end-to-end** (người dùng chủ động test sau) — cần **restart cả Gateway và Site** để thay đổi
    có hiệu lực (2 process đang chạy vẫn dùng policy cũ trong bộ nhớ).
- **Permission API (service Site)** — không phải entity/bảng mới, mà là **1 API tổng hợp giúp gán quyền nhanh**,
  làm rõ qua brainstorm: người dùng muốn 1 API "phân quyền từ site vào role/screen/function" thay vì phải gọi
  từng `CreateRoleFunction`/`DeleteRoleFunction` một. Route `api/permission`, dùng lại hoàn toàn
  `Module`/`Screen`/`Function`/`Role`/`Role_Function` sẵn có, không thêm bảng:
  - `GET /api/permission/{siteId}?roleId={roleId}` — trả cây phân cấp Module → Screen → Function của 1 Site (chỉ
    lấy các bản ghi `Status = 1`), mỗi Function có cờ `IsAssigned` (đã gán cho `roleId` truyền vào chưa, nếu không
    truyền `roleId` thì `IsAssigned` luôn `false`). Gộp 4 query phẳng (Modules by Site, Screens by ModuleIds,
    Functions by ScreenIds, assigned FunctionIds by RoleId) rồi ráp cây trong C#, tránh N+1 query.
  - `PUT /api/permission/role/{roleId}` (body `{ FunctionIds: [...] }`, gắn `[RequireFunction("AssignRolePermissions")]`)
    — đồng bộ toàn bộ Function của 1 Role trong **1 lần gọi + 1 transaction**: so sánh danh sách mong muốn với
    dữ liệu hiện có trong `tbl_Role_Function` (kể cả row đã soft-delete trước đó, để **reactivate** thay vì insert
    trùng — insert trùng sẽ vi phạm unique index `IX_tbl_Role_Function_FunctionId_RoleId` vì index không lọc theo
    Status), rồi insert mới / reactivate / soft-delete (`Status = -1`) đúng phần chênh lệch. Sau khi ghi DB, gọi
    `InvalidateUsersCacheByRole` (cùng pattern các repo khác) để xoá cache Redis của mọi user đang có Role đó.
  - `Site.Application/Interfaces/IPermissionRepository.cs`, `Features/Permission/{GetPermissionTree,AssignRolePermissions}`,
    `Site.Infrastructure/Repositories/PermissionRepository.cs` (mới), đăng ký DI, route YARP `permission-route`.
- **Chặn theo Function cụ thể (Function-level enforcement) — thay AdminPolicy cứng bằng RBAC thật.** Lý do: sau
  khi áp `AdminPolicy` (mục dưới), phát hiện ra vấn đề thực tế — nếu tạo Role tuỳ ý kiểu "KTV" (kỹ thuật viên) rồi
  gán Function "CreateRole" qua `Role_Function`, user KTV **vẫn không gọi được** `POST /api/role` vì `AdminPolicy`
  chỉ check literal `Role Claim == "Admin"`, hoàn toàn không đọc `Role_Function`. Đã xử lý:
  - Thêm `CommonLibs/Utils/RequireFunctionAttribute.cs` — attribute gắn lên action, chứa 1 `FunctionId`.
  - `CommonLibs/Middlewares/AuthorizationMiddleware.cs` (bước 9, sau khi đã nạp `allowedFunctionIds`): nếu action
    có `[RequireFunction("X")]`, user phải có `"X"` trong `allowedFunctionIds` (nạp từ Redis/SQL theo
    User_Role→Role_Function ở bước 8) mới được đi tiếp; user có Role literal "Admin" (toàn hệ thống) luôn bypass.
  - **Thay toàn bộ `[Authorize(Policy = "AdminPolicy")]` trên 31 action mutating** (Create/Update/Delete/Import ở
    10 controller, trừ `UpdateSite`/`DeleteSite` vẫn giữ `IsSiteAdmin` riêng) bằng `[RequireFunction("TênAction")]`
    — **quy ước**: FunctionId trùng chính xác tên method C# (VD `CreateRole`, `DeleteUser`, `ImportSites`), do
    người dùng chưa chỉ định quy ước khác nên tự chọn cách này (đơn giản, dễ trace ngược).
  - Class-level `[Authorize(Policy = "UserOrAdminPolicy")]` vẫn giữ nguyên trên mọi controller (bắt buộc phải
    login + có ít nhất 1 Role) — `RequireFunction` là lớp kiểm tra bổ sung, tự throw 403 riêng (không phải ASP.NET
    Core Policy, không cộng dồn AND với Authorize attribute khác).
  - **Việc còn lại (ngoài phạm vi bước 1+2 đã làm)**: FunctionId hiện chỉ là chuỗi quy ước trong code, **chưa có
    dữ liệu `tbl_Functions` nào thực sự khớp tên** (VD chưa có row `FunctionId = 'CreateRole'`) — người dùng cần
    tự tạo các Function này qua API Function đã có, đúng `FunctionId` khớp tên action, rồi gán cho Role tuỳ ý qua
    `Role_Function` thì user thuộc Role đó mới gọi được action tương ứng.
- **Áp dụng AdminPolicy cho các API mutating + hạ tầng Admin theo Site** — trả lời câu hỏi "policy có cần sửa gì
  không, ví dụ chỉ Admin mới được truy cập" bằng 2 phần (**lưu ý: mục 1 dưới đây sau đó đã được thay bằng
  `RequireFunction` ở bullet trên — chỉ còn đúng ở SiteController's Update/Delete và mục 2**):
  1. ~~AdminPolicy cho API mutating~~: trước đây toàn bộ 10 controller (User, Site, Module, Role, Function,
     RoleFunction, Screen, UserRole, UserSite) chỉ dùng `UserOrAdminPolicy` đồng loạt — bất kỳ user đã login nào
     cũng gọi được API quản trị quyền (`CreateRole`, `DeleteRoleFunction`, `CreateUserRole`...). Action đọc
     (GetById/GetAll/Export/GetXByY) và `ChangePassword` (tự đổi mật khẩu của chính mình) giữ nguyên
     `UserOrAdminPolicy`.
  2. **Admin theo Site (không chỉ Admin toàn hệ thống)**: `AuthorizationRepository.Login()` giờ tính thêm
     `adminSiteIds` — SiteId của các Site mà user có Role tên **"Admin"** (case-insensitive) — và emit claim
     `"admin_site"` (1 claim/site) kèm cache vào `UserPermissionCache.AdminSiteIds`. Thêm helper
     `BaseController<T>.IsSiteAdmin(siteId)` (`CommonLibs/Controllers/BaseController.cs`): true nếu user là Admin
     toàn hệ thống HOẶC có claim `admin_site` khớp đúng site đó. **Chỉ wire cụ thể vào `SiteController`** làm ví
     dụ mẫu: `UpdateSite`/`DeleteSite` dùng `IsSiteAdmin(siteId)` thay cho `AdminPolicy` toàn cục (cho phép "Admin
     của Site A" tự quản site của mình mà không cần là Admin toàn hệ thống); `CreateSite`/`ImportSites` vẫn dùng
     `AdminPolicy` toàn cục vì tạo site mới không có site context sẵn để so khớp.
  - **Chưa mở rộng site-scoping cho các controller khác** (Module/Role/Function/Screen/RoleFunction/UserRole/
    UserSite) — mỗi entity có cách suy ra SiteId khác nhau (trực tiếp như Module/Role, gián tiếp qua chuỗi
    Module→Screen→Function, hoặc không có SiteId nào như RoleFunction/UserRole) nên cần thiết kế riêng từng
    trường hợp nếu muốn, hiện các controller đó vẫn dùng `AdminPolicy` toàn cục cho hành động mutating.
- **Fix invalidate cache Redis chủ động khi sửa User_Role/Role_Function** — giải quyết giới hạn đã biết trước đó
  (cache chỉ hết hạn theo TTL 1h, không invalidate chủ động). `UserPermissionCache` cache theo key
  `auth:user-permissions:{userId}` (Roles+Functions của 1 user) nên cách xử lý khác nhau tuỳ bảng bị sửa ảnh
  hưởng 1 user hay nhiều user:
  - `UserRoleRepository` (`CreateUserRole`/`DeleteUserRole`/`ImportUserRoles`) — chỉ ảnh hưởng đúng 1 user (chính
    `UserId` trong request), nên `RemoveAsync` thẳng cache key của user đó ngay sau khi ghi DB thành công (Import
    thì lặp qua từng `UserId` distinct trong batch).
  - `RoleFunctionRepository` (`CreateRoleFunction`/`DeleteRoleFunction`/`ImportRoleFunctions`) — 1 Role có thể có
    **nhiều user** đang được gán (qua `User_Role`), nên thêm helper `InvalidateUsersCacheByRole(roleId, ct)`:
    query `Select Distinct UserId From tbl_User_Role Where RoleId = @RoleId And Status = 1` rồi `RemoveAsync`
    cache của từng user đó. Cả 3 method (Create/Delete/Import) đều gọi helper này sau khi ghi DB thành công
    (Import thì lặp qua từng `RoleId` distinct trong batch).
  - `RoleRepository` (`UpdateRole`/`DeleteRole`/`ImportRoles`) — cùng cơ chế `InvalidateUsersCacheByRole` như
    `RoleFunctionRepository` (RoleName/Status của Role đổi cũng ảnh hưởng cache y hệt việc đổi Function của Role
    đó). `CreateRole` không cần invalidate (role mới tạo chưa ai được gán).
  - `FunctionRepository` (`UpdateFunction`/`DeleteFunction`/`ImportFunctions`) — thêm helper
    `InvalidateUsersCacheByFunction(functionId, ct)`: join 2 tầng
    `tbl_Role_Function → tbl_User_Role` (`Where RF.FunctionId = @FunctionId And RF.Status = 1 And UR.Status = 1`)
    để tìm mọi user có Role đang được gán Function này, rồi xoá cache từng user. `CreateFunction` không cần
    invalidate.
  - Cả 4 repository (`UserRole`, `RoleFunction`, `Role`, `Function`) nhận thêm `IRedisService` qua constructor
    (đã đăng ký sẵn trong DI từ trước, không cần thêm registration mới).
  - Không còn giới hạn nào tồn đọng về invalidate cache cho toàn bộ bảng ảnh hưởng tới quyền
    (`User_Role`/`Role_Function`/`Role`/`Function`).
- **UserRole + UserSite API (service Site) — 2 entity cuối, Site service đã đủ 8/8 entity.** Cả 2 đều là bảng
  quan hệ N-N giống `RoleFunction`, áp dụng lại đúng 2 quyết định đã chốt trước đó (khóa route = cặp FK, bỏ API
  Update) + 2 quyết định mới hỏi riêng cho lần này:
  1. **API tiện ích lấy theo User**: `GET /api/user-role/{userId}` → `GetRolesByUser`, trả toàn bộ `RoleOutputDTO`
     user đang có (JOIN `tbl_User_Role`→`tbl_Roles`). `GET /api/user-site/{userId}` → `GetSitesByUser`, trả toàn
     bộ `SiteOutputDTO` user đang thuộc (JOIN `tbl_User_Site`→`tbl_Site`). Cùng hướng với `GetFunctionsByRole` đã
     làm cho RoleFunction (không làm chiều ngược lại GetUsersByRole/GetUsersBySite).
  2. **Ràng buộc nghiệp vụ (vá lỗ hổng đã nêu trong đánh giá RBAC)**: `UserRoleRepository.CreateUserRole` giờ
     kiểm tra Role đang gán thuộc `SiteId` nào (`tbl_Roles`), rồi bắt buộc user phải có `UserSite` **active** với
     đúng `SiteId` đó (`tbl_User_Site`) mới cho gán — nếu chưa, throw 400 "User must belong to the site... before
     being assigned this role." `CreateUserSite` (bảng gốc, không có tiền điều kiện) không có ràng buộc tương tự.
  - Route: `api/user-role` (`POST`, `GET /{userId}/{roleId}`, `GET /{userId}`, `GET ""`, `GET /export`,
    `DELETE /{userId}/{roleId}`, `POST /import-user-roles`) và `api/user-site` (tương tự, đổi `roleId`→`siteId`) —
    kebab-case ngay từ đầu, không phải đổi lại như `rolefunction`.
  - Infrastructure: `Site.Infrastructure/Repositories/{UserRoleRepository,UserSiteRepository}.cs` (mới), bảng
    `tbl_User_Role`/`tbl_User_Site`, đăng ký DI. Sửa `IUserRoleRepository`/`IUserSiteRepository`: cùng bug đặt
    sai tên tham số `siteId` như các interface trước, bỏ `UpdateUserRole`/`UpdateUserSite`.
  - Template import mẫu: `CommonLibs/TemplatesImport/Import_UserRoles_mau.xlsx` và `Import_UserSites_mau.xlsx`.
  - `Gateway/appsettings.json`: thêm route YARP `user-role-route` và `user-site-route`.
- **Kết nối RBAC (Role/RoleFunction) vào authorization thật, cache qua Redis** — trước đây toàn bộ Site/Module/
  Role/Function/RoleFunction/Screen API chỉ là CRUD quản trị dữ liệu, **không hề ảnh hưởng tới việc user thực sự
  được phép làm gì** (`Login()` hard-code `ClaimTypes.Role = "User"`, `AdminPolicy` định nghĩa nhưng không dùng ở
  đâu). Bước 1 (đã làm, xem lịch sử) từng dùng quy ước "RoleName chứa admin" để suy ra Admin/User — sau đó được
  yêu cầu sửa lại: **không gán cứng/suy đoán Admin-User, phải gán đúng theo Role thật của user, cache Role +
  Function vào Redis rồi đọc ngược ra** (thay vì query SQL trực tiếp mỗi request). Thiết kế cuối:
  1. `Gateway.Infrastructure/Repositories/AuthorizationRepository.cs` — `Login()` query toàn bộ `RoleName` đang
     active của user (`tbl_User_Role Inner Join tbl_Roles`) và toàn bộ `FunctionId` được phép (join thêm
     `tbl_Role_Function`). Mỗi `RoleName` tạo **1 claim `ClaimTypes.Role` riêng** (JWT/`ClaimsPrincipal` hỗ trợ
     nhiều Role claim cùng lúc — không còn ép về 1 giá trị "Admin"/"User" duy nhất). Đồng thời `SetAsync` cả
     Role+Function vào Redis, key `auth:user-permissions:{userId}` (định nghĩa tại `UserPermissionCache.GetCacheKey`,
     `CommonLibs/Utils/UserPermissionCache.cs`), TTL 1h khớp thời hạn access token.
  2. `CommonLibs/Middlewares/AuthorizationMiddleware.cs` — đọc **toàn bộ** Role claim (không chỉ lấy claim đầu
     tiên như code cũ) để forward header `X-User-Role` (comma-separated) và check policy đa-role đúng. Danh sách
     Function được phép: **đọc Redis trước** (`redisService.GetAsync<UserPermissionCache>`) bằng key đã cache từ
     lúc `Login()`; nếu cache miss/hết hạn mới fallback query SQL trực tiếp (`ISqlConnectionFactory`) rồi ghi lại
     vào Redis (read-through) cho request sau. Set vào `context.Items["allowedFunctions"]` + header
     `X-User-Functions`, vẫn **chưa chặn cứng theo endpoint** (Function chưa gắn với route cụ thể nào — xem việc
     tiếp theo).
  - **Hệ quả cần biết khi test**: user không có `User_Role` nào active sẽ **không có claim Role nào cả** →
     `UserOrAdminPolicy`/`AdminPolicy` fail ngay (403), khác hẳn hành vi cũ (luôn "User"). Cần seed `User_Role`
     cho mọi tài khoản test, và Role muốn coi là Admin phải có `RoleName` **đúng literal "Admin"** (vì
     `AdminPolicy` = `RequireRole("Admin")` so khớp giá trị claim, không còn suy luận qua substring nữa).
  - ~~Giới hạn: cache Redis không invalidate chủ động khi sửa `User_Role`/`Role_Function`~~ — **đã fix**, xem bullet
    "Fix invalidate cache Redis chủ động..." phía trên (mục mới nhất).
- **Screen API (service Site)** — entity `Screen` (thuộc 1 `Module` qua FK `ModuleId`), cấu trúc/route giống
  Module/Role/Function (`ScreenId`, `ScreenName`, `ModuleId` + audit fields, bảng `tbl_Screens`, route flat
  `/api/screen`):
  - Domain: bổ sung `HeaderMappings`, `ImportScreensInputDTO`, `DeleteScreenInputDTO` vào `ScreenDTO.cs`.
  - Application: đầy đủ CQRS tại `Features/Screen/{CreateScreen,UpdateScreen,GetScreenById,GetAllScreens,DeleteScreen,ImportScreens,ExportScreens}`.
    Sửa `IScreenRepository`: cùng bug đặt sai tên tham số `siteId` → `screenId` như các interface trước,
    `DeleteScreen` nhận `DeleteScreenInputDTO`, `ImportScreens` nhận thêm `fileName` + `CancellationToken`.
  - Infrastructure: tạo mới `Site.Infrastructure/Repositories/ScreenRepository.cs` (trước đó chưa tồn tại),
    đăng ký DI.
  - Presentation: `Site/Controllers/ScreenController.cs` (mới).
  - Template import mẫu: `CommonLibs/TemplatesImport/Import_Screens_mau.xlsx`.
  - `Gateway/appsettings.json`: thêm route YARP `screen-route` (`/api/screen/{**catch-all}` → `site-cluster`,
    policy `UserOrAdminPolicy`).
- **RoleFunction API (service Site)** — đây là bảng **quan hệ N-N** (`RoleId` + `FunctionId`, không có khóa
  nghiệp vụ đơn riêng, chỉ có `AutoId` tự tăng), khác với Module/Role/Function nên đã hỏi lại 2 quyết định thiết
  kế trước khi làm: (1) dùng **cặp `RoleId`+`FunctionId`** làm khóa route thay vì `AutoId`; (2) **bỏ API Update**
  (bảng mapping chỉ nên gắn/gỡ, không "update" — khác các entity đơn trước đó), chỉ còn 6 method CRUD-style
  + 1 API tiện ích riêng (`GetFunctionsByRole`). Route dùng kebab-case `api/role-function` (đổi từ
  `api/rolefunction` ban đầu theo yêu cầu sau đó):
  - `POST /api/role-function`, `GET /api/role-function/{roleId}/{functionId}`, `GET /api/role-function`,
    `GET /api/role-function/export`, `DELETE /api/role-function/{roleId}/{functionId}`,
    `POST /api/role-function/import-role-functions`.
  - `GET /api/role-function/{roleId}` (mới, thêm sau) — trả về **toàn bộ `FunctionOutputDTO`** (không phải
    `RoleFunctionOutputDTO`) đang gán cho 1 Role, qua JOIN `tbl_Functions` với `tbl_Role_Function` (lọc
    `RF.Status = 1 And F.Status = 1`). Khác `GET /api/role-function?RoleId=X` (trả về các dòng mapping thô, chỉ
    có `RoleId`+`FunctionId`) — endpoint mới trả chi tiết Function thật sự hữu ích hơn cho UI phân quyền.
  - Domain: bổ sung `HeaderMappings` vào `RoleFunctionInputDTO`, thêm `ImportRoleFunctionsInputDTO`,
    `DeleteRoleFunctionInputDTO` (chứa cả `RoleId` + `FunctionId` + `IsSoftDelete`, khác pattern 1-field như các
    entity đơn) vào `RoleFunctionDTO.cs`.
  - Application: `Features/RoleFunction/{CreateRoleFunction,GetRoleFunctionById,GetAllRoleFunctions,DeleteRoleFunction,ImportRoleFunctions,ExportRoleFunctions,GetFunctionsByRole}`
    (không có `UpdateRoleFunction`). Sửa `IRoleFunctionRepository`: bỏ `UpdateRoleFunction`,
    `GetRoleFunctionById`/`DeleteRoleFunction` dùng composite key thay vì 1 tham số `siteId` sai như bug cũ, thêm
    `GetFunctionsByRole(string roleId, ...)`.
  - Infrastructure: tạo mới `Site.Infrastructure/Repositories/RoleFunctionRepository.cs`, bảng `tbl_Role_Function`,
    match/merge theo cặp `RoleId+FunctionId`, đăng ký DI. Dùng alias `FunctionEntity = Site.Domain.Entities.Function`
    để query JOIN sang `tbl_Functions` cho `GetFunctionsByRole`.
  - Presentation: `Site/Controllers/RoleFunctionController.cs` (mới).
  - Template import mẫu: `CommonLibs/TemplatesImport/Import_RoleFunctions_mau.xlsx`.
  - `Gateway/appsettings.json`: route YARP `role-function-route` (`/api/role-function/{**catch-all}` →
    `site-cluster`, policy `UserOrAdminPolicy`) — đã đổi tên route id + path theo route controller đổi sang
    kebab-case.
- **Function API (service Site)** — entity `Function` (thuộc 1 `Screen` qua FK `ScreenId`, không phải `SiteId`
  trực tiếp), cấu trúc/route giống Module/Role (`FunctionId`, `FunctionName`, `ScreenId` + audit fields, bảng
  `tbl_Functions`, route flat `/api/function`):
  - Domain: bổ sung `HeaderMappings`, `ImportFunctionsInputDTO`, `DeleteFunctionInputDTO` vào `FunctionDTO.cs`.
  - Application: đầy đủ CQRS tại `Features/Function/{CreateFunction,UpdateFunction,GetFunctionById,GetAllFunctions,DeleteFunction,ImportFunctions,ExportFunctions}`.
    Sửa `IFunctionRepository`: cùng bug đặt sai tên tham số `siteId` → `functionId` như các interface trước,
    `DeleteFunction` nhận `DeleteFunctionInputDTO`, `ImportFunctions` nhận thêm `fileName` + `CancellationToken`.
  - Infrastructure: tạo mới `Site.Infrastructure/Repositories/FunctionRepository.cs` (trước đó chưa tồn tại),
    đăng ký DI.
  - Presentation: `Site/Controllers/FunctionController.cs` (mới).
  - Template import mẫu: `CommonLibs/TemplatesImport/Import_Functions_mau.xlsx`.
  - `Gateway/appsettings.json`: thêm route YARP `function-route` (`/api/function/{**catch-all}` → `site-cluster`,
    policy `UserOrAdminPolicy`).
- **Role API (service Site)** — entity `Role` (thuộc 1 `Site` qua FK `SiteId`), cấu trúc/route giống hệt Module
  (`RoleId`, `RoleName`, `SiteId` + audit fields, bảng `tbl_Roles`, route flat `/api/role`):
  - Domain: bổ sung `HeaderMappings`, `ImportRolesInputDTO`, `DeleteRoleInputDTO` vào `RoleDTO.cs`.
  - Application: đầy đủ CQRS tại `Features/Role/{CreateRole,UpdateRole,GetRoleById,GetAllRoles,DeleteRole,ImportRoles,ExportRoles}`.
    Sửa `IRoleRepository`: tham số `GetRoleById`/`DeleteRole` đổi tên đúng `roleId` (trước đó bug đặt tên `siteId`,
    cùng lỗi copy-paste như `IModuleRepository`), `DeleteRole` nhận `DeleteRoleInputDTO`, `ImportRoles` nhận thêm
    `fileName` + `CancellationToken`.
  - Infrastructure: tạo mới `Site.Infrastructure/Repositories/RoleRepository.cs` (trước đó chưa tồn tại), đăng ký DI.
  - Presentation: `Site/Controllers/RoleController.cs` (mới).
  - Template import mẫu: `CommonLibs/TemplatesImport/Import_Roles_mau.xlsx`.
  - `Gateway/appsettings.json`: thêm route YARP `role-route` (`/api/role/{**catch-all}` → `site-cluster`, policy
    `UserOrAdminPolicy`).
- **Module API (service Site)** — entity `Module` (thuộc 1 `Site` qua FK `SiteId`), route flat `/api/module`
  (không nested dưới `/api/site`):
  - Domain: bổ sung `HeaderMappings`, `ImportModulesInputDTO`, `DeleteModuleInputDTO` vào `ModuleDTO.cs`.
  - Application: đầy đủ CQRS tại `Features/Module/{CreateModule,UpdateModule,GetModuleById,GetAllModules,DeleteModule,ImportModules,ExportModules}`.
    Sửa `IModuleRepository`: tham số `GetModuleById`/`DeleteModule` đổi tên đúng `moduleId` (trước đó bug đặt tên
    `siteId`), `DeleteModule` nhận `DeleteModuleInputDTO`, `ImportModules` nhận thêm `fileName` + `CancellationToken`.
  - Infrastructure: tạo mới `Site.Infrastructure/Repositories/ModuleRepository.cs` (trước đó chưa tồn tại), bảng
    `tbl_Modules`, đăng ký DI. Lưu ý: dùng alias `using ModuleEntity = Site.Domain.Entities.Module;` để tránh nhầm
    lẫn tên (không bắt buộc như case `Site` nhưng giữ nhất quán phòng ngừa).
  - Presentation: `Site/Controllers/ModuleController.cs` (mới).
  - Template import mẫu: `CommonLibs/TemplatesImport/Import_Modules_mau.xlsx`.
  - `Gateway/appsettings.json`: thêm route YARP `module-route` (`/api/module/{**catch-all}` → `site-cluster`,
    policy `UserOrAdminPolicy`) để gọi được Module qua Gateway, giống `site-route`.
- Template import mẫu cho Site: `CommonLibs/TemplatesImport/Import_Sites_mau.xlsx` (tạo sau khi đã làm xong Site
  API, cùng cấu trúc header với `SiteInputDTO.HeaderMappings`).
- **User API (Gateway)**:
  - Mã hóa password khi `ImportUsers` (dùng `EncryptionUtils`, cùng cách `CreateUser`/`ChangePassword`).
  - `POST /api/user/import-users` (nhận file csv/xlsx qua `IFormFile`) + `ImportUsersCommand`/`Handler`.
  - Template import mẫu tại `CommonLibs/TemplatesImport/Import_User_mau.xlsx`.
  - `DELETE /api/user/{userId}` (soft/hard delete qua `DeleteUserInputDTO.IsSoftDelete`) + `DeleteUserCommand`/`Handler`.
  - `PUT /api/user/{userId}` (Update) + `UpdateUserCommand`/`Handler`.
- **Site API (service Site)** — chỉ mới làm entity `Site` (Module/Role/Screen/Function/RoleFunction/UserRole/UserSite
  **chưa làm**, xem phần "Việc tiếp theo"):
  - Domain: bổ sung `HeaderMappings`, `ImportSitesInputDTO`, `DeleteSiteInputDTO` vào `SiteDTO.cs`.
  - Application: đầy đủ CQRS tại `Features/Site/{CreateSite,UpdateSite,GetSiteById,GetAllSites,DeleteSite,ImportSites,ExportSites}`.
  - Infrastructure: implement đầy đủ `SiteRepository.cs` (trước đó toàn `NotImplementedException`), đăng ký DI.
  - Presentation: `Site/Controllers/SiteController.cs` (mới) — CRUD + import/export, theo đúng pattern `UserController`.
- **Fix bug build**: `CommonLibs/BaseDbContext/BaseDbContext.cs` — lỗi CS1997 do `return await strategy.ExecuteAsync(...)`
  trong method trả về `Task` (non-generic, không được `return` kèm giá trị).
- **Fix Swagger gộp qua Gateway (YARP)**:
  - `Gateway/appsettings.json`: cluster `site-cluster` dùng sai giao thức `https://localhost:8001/` (Site chỉ chạy
    HTTP) → sửa thành `http://`.
  - `Gateway/Program.cs`: `SwaggerEndpoint` trỏ sai path `/site-api/swagger/...` (không khớp route YARP nào) → sửa
    thành `/site-swagger/v1/swagger.json` cho khớp route `site-swagger-route`.
- **Swagger Site**: thêm tự động header `X-Device-ID`/`X-Device-Name` giống Gateway — tạo
  `Site/Utils/AddRequiredHeaderParameter.cs` + đăng ký `OperationFilter` trong `Site/Program.cs`.
- **Fix lỗi 401 khi Gateway proxy sang Site**: chuyển `AuthorizationMiddleware` từ
  `Gateway.Application/Middlewares/` sang dùng chung tại `CommonLibs/Middlewares/AuthorizationMiddleware.cs`,
  đăng ký ở cả `Gateway/Program.cs` và `Site/Program.cs` (sau `UseAuthentication`, trước `UseAuthorization`) —
  mỗi service tự verify JWT + role + device độc lập (defense in depth), không đánh đổi bảo mật dù client luôn đi
  qua Gateway.

### Việc tiếp theo / còn tồn đọng

- **API refresh token (`POST /api/authorization/renew-token`, mục mới nhất phía trên) chưa được test lại** —
  cần restart Gateway rồi test cả happy path (access token hết hạn + refresh token còn hạn trong cookie → nhận
  access+refresh token mới) lẫn edge case (access token còn hạn → phải bị từ chối; refresh token sai/hết hạn →
  phải bị từ chối).
- **Fix 502 ASCII header + fix `UserOrAdminPolicy` (2 mục phía trên) chưa được test lại cùng nhau** — cần
  **restart cả Gateway và Site** rồi gọi lại đúng request đã fail 2 lần liên tiếp (`GET /api/permission/{siteId}
  ?roleId=...` với token user có Role không phải literal "Admin", VD "Ký thuật viên") để xác nhận lần này response
  vào tới controller và trả đúng chuẩn `Response<T>` (không còn 403 rỗng, không còn 502). Sau khi fix
  `UserOrAdminPolicy`, lưu ý các test case cũ giả định user cần Role literal `"User"` mới qua được policy này sẽ
  không còn đúng nữa — giờ chỉ cần có **bất kỳ** Role claim nào.
- **Toàn bộ 8/8 entity của Site service đã có API đầy đủ** (Site, Module, Role, Function, RoleFunction, Screen,
  UserRole, UserSite) — không còn entity nào thiếu Repository/Controller trong service Site nữa.
- Route YARP `site-route` trong `Gateway/appsettings.json` có `Match.Path: "api/site/{**catch-all}"` thiếu dấu
  `/` đầu so với các route khác — chưa xác nhận có gây lỗi thật hay không, cần kiểm tra khi test end-to-end qua
  Gateway.
- Port 8001 của Site đang `ListenAnyIP` (mở trên mọi network interface) — cần đảm bảo ở tầng hạ tầng/firewall
  không public port này ra ngoài, chỉ cho phép gọi nội bộ (không phải việc code có thể tự fix).
- Các API mới thêm (Import/Delete/Update User, toàn bộ 8 entity Site, luồng Gateway → Site với
  `AuthorizationMiddleware` + Redis cache mới) đang chờ người dùng tự test lại theo `06-testing.md` (tài khoản
  `admin/admin2604`), chưa có xác nhận kết quả test cuối cùng. Nhớ seed `User_Role`/`User_Site` cho tài khoản
  test trước khi thử `CreateUserRole` (sẽ fail nếu user chưa có `UserSite` khớp `SiteId` của Role).
- **Quan trọng khi test sau khi áp `RequireFunction`**: hầu hết action Create/Update/Delete/Import (trừ
  ChangePassword) giờ đòi hỏi user **hoặc** có Role literal **"Admin"** (bypass toàn cục) **hoặc** có Role được
  gán đúng `FunctionId` (trùng tên action, VD `CreateRole`) qua `Role_Function` — tài khoản test cần 1 trong 2
  điều kiện này trước khi test API mutating, nếu không sẽ luôn nhận 403 Forbidden. Muốn test theo hướng "role tuỳ
  ý + function cụ thể" (không phải Admin toàn hệ thống) thì cần tự tạo Function khớp tên action qua API Function
  đã có, gán cho Role qua `Role_Function`, rồi gán Role đó cho user qua `User_Role`. Riêng `UpdateSite`/`DeleteSite`
  vẫn dùng `IsSiteAdmin` (Admin toàn hệ thống hoặc Admin riêng của đúng site đó), không dùng `RequireFunction`.
- Chưa mở rộng `RequireFunction` cho `UpdateSite`/`DeleteSite` (vẫn thuần `IsSiteAdmin`) — nếu muốn "KTV" cũng
  thao tác được Site cụ thể qua Function-grant (không chỉ qua Admin/site-admin) thì cần thiết kế thêm.
