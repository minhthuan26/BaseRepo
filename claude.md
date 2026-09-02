# BaseRepo - Developer Guide

## 🏗️ Tổng quan kiến trúc

Project **BaseRepo** là một hệ thống Microservices backend sử dụng **.NET 10**, được xây dựng theo kiến trúc **Clean Architecture** kết hợp **CQRS (Command Query Responsibility Segregation)** thông qua thư viện **MediatR**.

### Công nghệ chính

| Thành phần | Công nghệ |
|---|---|
| Framework | .NET 10, ASP.NET Core Web API |
| Architecture | Clean Architecture + CQRS |
| Mediator | MediatR |
| ORM / Data Access | Entity Framework Core, Dapper |
| Mapping | AutoMapper |
| Database | SQL Server (Stored Procedures + Raw SQL) |
| API Gateway | YARP (Yet Another Reverse Proxy) |
| Authentication | JWT Bearer Token |
| Logging | log4net, Dynamic Logging |
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
├── CommonLibs/                       # 🔧 Shared Library (dùng chung cho MỌI domain — Base lẫn SportsBooking,
│   │                                 #    BloodDonation... — nằm ở gốc repo, không thuộc riêng nhóm nào)
│   ├── BaseDbContext/                # Base DbContext với Dapper extensions
│   │   └── BaseDbContext.cs
│   ├── BaseRabbitMQ/                 # RabbitMQ base consumer
│   ├── BaseSQL/                      # SQL helpers
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
├── Base/                             # 🧱 Nhóm nền tảng platform (auth, RBAC, danh mục hành chính) — dùng chung
│   │                                 #    cho mọi domain, không thuộc riêng dự án nghiệp vụ nào
│   ├── Gateway/                      # 🌐 Service Gateway — 5 project con gom vào 1 folder cha
│   │   ├── Gateway.Api/              # 🌐 API Gateway (Presentation Layer)
│   │   │   ├── Controllers/
│   │   │   │   ├── AuthorizationController.cs
│   │   │   │   ├── UserController.cs
│   │   │   │   └── TestGatewayAPIController.cs
│   │   │   ├── Utils/
│   │   │   ├── Keys/                 # JWT signing keys
│   │   │   ├── Program.cs            # Entry point + DI + YARP + Auth config
│   │   │   ├── appsettings.json      # Config (YARP routes, DB, JWT...)
│   │   │   └── log4net.config
│   │   │
│   │   ├── Gateway.Application/      # 📋 Application Layer (CQRS Handlers)
│   │   │   ├── Features/             # Feature-based organization
│   │   │   │   ├── Authorization/
│   │   │   │   │   ├── Login/
│   │   │   │   │   │   ├── LoginCommand.cs
│   │   │   │   │   │   └── LoginCommandHandler.cs
│   │   │   │   │   └── Logout/
│   │   │   │   │       ├── LogoutCommand.cs
│   │   │   │   │       └── LogoutCommandHandler.cs
│   │   │   │   └── User/
│   │   │   │       ├── CreateUser/
│   │   │   │       │   ├── CreateUserCommand.cs
│   │   │   │       │   └── CreateUserCommandHandler.cs
│   │   │   │       ├── GetAllUsers/
│   │   │   │       │   ├── GetAllUsersQuery.cs
│   │   │   │       │   └── GetAllUsersQueryHandler.cs
│   │   │   │       └── GetUserById/
│   │   │   │           ├── GetUserByIdQuery.cs
│   │   │   │           └── GetUserByIdQueryHandler.cs
│   │   │   ├── Interfaces/           # Repository interfaces
│   │   │   │   ├── IAuthorizationRepository.cs
│   │   │   │   └── IUserRepository.cs
│   │   │   ├── Mappings/             # AutoMapper profiles
│   │   │   │   └── GatewayMapping.cs
│   │   │   └── Middlewares/
│   │   │       └── AuthorizationMiddleware.cs
│   │   │
│   │   ├── Gateway.Contract/         # 📦 Contract Layer (DTOs — tách khỏi Domain)
│   │   │   └── DTOs/
│   │   │       ├── AuthorizationDTO.cs
│   │   │       └── UserDTO.cs        # UserInputDTO, UserOutputDTO, UserFilterDTO
│   │   │
│   │   ├── Gateway.Domain/           # 🏛️ Domain Layer (chỉ còn Entities — DTO đã chuyển sang Gateway.Contract)
│   │   │   └── Entities/
│   │   │       └── User.cs           # Rich Domain Entity với guard methods
│   │   │
│   │   └── Gateway.Infrastructure/   # 🔌 Infrastructure Layer (Data Access)
│   │       ├── DBContext/
│   │       │   └── GatewayDbContext.cs   # DbContext kế thừa BaseContext
│   │       ├── Repositories/
│   │       │   ├── AuthorizationRepository.cs
│   │       │   └── UserRepository.cs     # Implement IUserRepository
│   │       └── DependencyInjection.cs    # Extension method đăng ký DI
│   │
│   ├── Site/                         # 🏥 Service Site — cùng khuôn 5 project như Gateway
│   │   ├── Site.Api/
│   │   ├── Site.Application/
│   │   ├── Site.Contract/
│   │   ├── Site.Domain/
│   │   └── Site.Infrastructure/
│   │
│   └── AdministrativeUnit/           # 🗺️ Service AdministrativeUnit — cùng khuôn 5 project (Province + Ward)
│       ├── AdministrativeUnit.Api/
│       ├── AdministrativeUnit.Application/
│       ├── AdministrativeUnit.Contract/
│       ├── AdministrativeUnit.Domain/
│       └── AdministrativeUnit.Infrastructure/
│
├── SportsBooking/                    # 🏸 Nhóm domain "Quản lý thuê sân thể thao"
│   └── Booking/                      # Service Booking — cùng khuôn 5 project
│       ├── Booking.Api/
│       ├── Booking.Application/
│       ├── Booking.Contract/
│       ├── Booking.Domain/
│       └── Booking.Infrastructure/
│
├── BloodDonation/                    # 🩸 Nhóm domain "Đăng ký hiến máu" — chưa có service, tạo trước làm chỗ chứa
│
├── Elastic/                          # 📊 Elasticsearch + Kibana + Filebeat tools
│
└── SystemManagement/                 # 🖥️ Windows desktop management tool (WinForms) — không thuộc kiến trúc
    ├── Config/                       #    5-layer này, giữ nguyên như cũ
    ├── Interfaces/
    ├── Models/
    └── Views/
```

---

## 🧱 Kiến trúc 5 Layer (cho mỗi Microservice)

Cả 3 service (`Gateway`, `Site`, `AdministrativeUnit`) đều theo cùng 1 khuôn 5 layer dưới đây kể từ 2026-09-01
(xem Progress Log để biết lịch sử tách từng service). Khi thêm service mới, tạo đủ 5 project ngay từ đầu theo
đúng khuôn này — không tạo kiểu 4-layer cũ (DTO trong Domain) nữa.

```
┌─────────────────────────────────────────────────────┐
│                 Presentation Layer                    │
│         ({ServiceName}.Api, VD: Gateway.Api)          │
│         Controllers, Program.cs, Config              │
├─────────────────────────────────────────────────────┤
│                 Application Layer                     │
│              ({ServiceName}.Application)               │
│    Features/ (Commands + Queries + Handlers)          │
│    Interfaces/ (Repository Contracts)                 │
│    Mappings/ (AutoMapper Profiles)                    │
├─────────────────────────────────────────────────────┤
│                 Contract Layer                        │
│               ({ServiceName}.Contract)                 │
│    DTOs/ (InputDTO, OutputDTO, FilterDTO)             │
├─────────────────────────────────────────────────────┤
│                  Domain Layer                         │
│                ({ServiceName}.Domain)                  │
│    Entities/ (Rich Domain Models)                     │
├─────────────────────────────────────────────────────┤
│               Infrastructure Layer                    │
│            ({ServiceName}.Infrastructure)              │
│    DBContext/ (EF Core DbContext)                     │
│    Repositories/ (Data Access Implementation)         │
│    DependencyInjection.cs (DI Registration)           │
└─────────────────────────────────────────────────────┘
              ↕ Dependency: Chỉ hướng xuống ↕
```

### Dependency Rule
- **Presentation** → Application, Contract, Domain, Infrastructure
- **Application** → Contract (DTOs), Domain (Entities), CommonLibs
- **Infrastructure** → Application (Interfaces), Contract (DTOs), Domain (Entities), CommonLibs
- **Contract** → CommonLibs (chỉ Base DTOs) — **không** được reference Domain/Application/Infrastructure
- **Domain** → CommonLibs (chỉ Base classes)
- **CommonLibs** → Không phụ thuộc vào service nào

---

## 📏 Quy tắc đặt tên (Naming Conventions)

### Projects
```
{ServiceName}.Api                → Presentation Layer  (VD: Gateway.Api, Site.Api, AdministrativeUnit.Api)
{ServiceName}.Application        → Application Layer
{ServiceName}.Contract           → Contract Layer (DTO Input/Output/Filter)
{ServiceName}.Domain             → Domain Layer (Entities)
{ServiceName}.Infrastructure     → Infrastructure Layer
```

### Cấu trúc thư mục vật lý

Kể từ 2026-09-01, mỗi service nằm trong 2 cấp thư mục: `{DomainGroup}/{ServiceName}/` — `DomainGroup` là
`Base/` cho các service nền tảng dùng chung (hiện có `Gateway`, `Site`, `AdministrativeUnit`), hoặc tên domain
nghiệp vụ riêng (VD `SportsBooking/` cho service `Booking`, `BloodDonation/` cho dự án "Đăng ký hiến máu" sắp
tới) — xem Progress Log mục "Tái cấu trúc thư mục repo theo nhóm domain" để biết lý do. `CommonLibs/` KHÔNG nằm
trong bất kỳ `DomainGroup` nào — luôn ở gốc repo vì dùng chung cho tất cả.

Cả 5 project của 1 service được gom vào 1 folder cha cùng tên service — tên project/namespace của
Application/Contract/Domain/Infrastructure giữ nguyên hậu tố `.Application`/`.Contract`/`.Domain`/`.Infrastructure`
như cũ; riêng Presentation đổi tên từ bare `{ServiceName}` thành `{ServiceName}.Api` (namespace
`{ServiceName}.Controllers`/`{ServiceName}.Utils` → `{ServiceName}.Api.Controllers`/`{ServiceName}.Api.Utils`):
```
{DomainGroup}/{ServiceName}/
├── {ServiceName}.Api/                (Presentation)
├── {ServiceName}.Application/
├── {ServiceName}.Contract/
├── {ServiceName}.Domain/
└── {ServiceName}.Infrastructure/
```

**Tạo service mới**: trước tiên xác định service thuộc `DomainGroup` nào — `Base/` nếu là hạ tầng dùng chung
(auth, RBAC, danh mục dùng chung...), hoặc domain nghiệp vụ tương ứng (tạo `DomainGroup/` mới nếu dự án đó chưa
có service nào). Mọi `ProjectReference` từ service mới trỏ tới `CommonLibs` phải dùng `..\..\..\CommonLibs\
CommonLibs.csproj` (3 cấp: `{ServiceName}.{Layer}` → `{ServiceName}` → `{DomainGroup}` → gốc repo).

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

## 📝 Cấu hình Serilog (appsettings.Serilog.json)

Kể từ 2026-09-01, section `"Serilog"` được tách ra khỏi `appsettings.json` thành file riêng
`{ServiceName}.Api/appsettings.Serilog.json` (áp dụng cho cả 3 service: Gateway, Site, AdministrativeUnit) —
comment trong section gốc được giữ nguyên khi tách. File mới match glob `appsettings.*.json` nên SDK Web tự copy
ra output directory, không cần khai báo gì thêm trong `.csproj`. Bắt buộc phải nạp thủ công trong `Program.cs`
(ASP.NET Core mặc định chỉ tự nạp `appsettings.json`/`appsettings.{Environment}.json`, không tự biết file mới),
đặt **trước** `builder.AddDynamicLogging()` vì đó là chỗ đọc section `"Serilog"` từ `builder.Configuration`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Serilog.json", optional: false, reloadOnChange: true);

builder.AddDynamicLogging();
```

Khi tạo service mới, áp dụng ngay từ đầu thay vì để `"Serilog"` nằm trong `appsettings.json` chính.

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

1. **Domain Layer**: Tạo Entity trong `{ServiceName}.Domain/Entities/`. DTOs (Input, Output, Filter) tạo ở
   `{ServiceName}.Contract/DTOs/` (Contract layer riêng, không còn trong Domain).
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

*Cập nhật gần nhất: 2026-09-02*

### Đã hoàn thành

- **Di chuyển file SQL cài đặt/seed từ `CommonLibs/BaseSQL/` vào `SQL/` của từng service sở hữu**
  (2026-09-02). Chuẩn bị hạ tầng cho tính năng "SQL" trong SystemManagement (đọc SQL trực tiếp từ thư
  mục triển khai của từng service qua `ModuleItem.FolderPath` có sẵn trong `SystemManagement/Models/
  SystemConfig.cs` — màn hình `UC_ConfigSQL` chưa làm, để người dùng tự làm sau, xem spec).
  - `CommonLibs/BaseSQL/BaseSQL.sql` + `seed_admin_account.sql` → `Base/Gateway/Gateway.Api/SQL/`.
  - `CommonLibs/BaseSQL/administrative-unit/{BaseAdministrativeUnit.sql,seed_administrative_unit.sql,README.md}`
    → `Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/` (+ sửa 4 chỗ tham chiếu path cũ ngay trong
    2 file `.sql` đã di chuyển).
  - `CommonLibs/BaseSQL/booking/BaseBooking.sql` → `SportsBooking/Booking/Booking.Api/SQL/`.
  - **Không di chuyển** `CommonLibs/BaseSQL/multi-tenant/` (tài liệu + test cách ly 2 Site, không phải
    file cài đặt) — chỉ sửa 4 chỗ path cũ trong `multi-tenant/README.md` cho khớp vị trí `BaseSQL.sql`
    mới.
  - Mỗi `.Api.csproj` (Gateway/AdministrativeUnit/Booking) thêm `<Content Include="SQL\**"
    CopyToOutputDirectory="PreserveNewest" />` — file `.sql`/`.md` không tự copy ra `bin/` như
    `appsettings.*.json`/`log4net.config`, phải khai rõ.
  - Spec: `docs/superpowers/specs/2026-09-02-sql-file-relocation-and-system-management-sql-tool-design.md`,
    plan: `docs/superpowers/plans/2026-09-02-sql-file-relocation-plan.md`.
  - `dotnet build BaseRepo.slnx`: 0 Error, và đã xác nhận cả 6 file `.sql`/`.md` xuất hiện đúng trong
    `bin/Debug/net10.0/SQL/` của 3 service tương ứng.
- **Đổi URL scheme YARP sang tiền tố theo TÊN SERVICE + gộp route cho cả 3 cluster** (2026-09-01,
  **CHƯA TEST** — môi trường hiện tại không đủ SQL Server/Redis để chạy thật, người dùng sẽ test
  ở máy khác). Xuất phát từ câu hỏi: mỗi lần thêm Controller mới đều phải nhớ sửa
  `Gateway/appsettings.json`, dễ quên (kỹ sư không nắm được) khiến API mới build/chạy service OK
  nhưng không gọi được qua Gateway. Bước 1 chỉ gộp được `site-cluster` (8 route trùng lặp → 1
  catch-all); phát hiện ra `administrativeunit-cluster`/`booking-cluster` không gộp thêm được vì
  mỗi entity là 1 path literal riêng ở gốc `/api/`, không có tiền tố chung theo service — dẫn tới
  quyết định đổi hẳn URL scheme.
  - **URL scheme mới**: mọi endpoint (trừ Gateway tự host như `/api/user`, `/api/authorization/login`)
    giờ có tiền tố tên SERVICE, không phải domain-group (`Base`/`SportsBooking` gộp nhiều service
    khác nhau nên không đủ để YARP phân biệt đích đến): `/api/site/...` (site-cluster),
    `/api/administrativeunit/...` (administrativeunit-cluster), `/api/booking/...` (booking-cluster).
    VD cũ `/api/ward` → mới `/api/administrativeunit/ward`, `/api/field` → `/api/booking/field`.
    **Breaking change API contract** — FE phải cập nhật base path theo từng service.
  - **Cơ chế**: mỗi route YARP dùng `"Transforms": [{ "PathPattern": "/api/{**catch-all}" }]` để bóc
    tiền tố service ra trước khi forward xuống service con — **không cần đổi route attribute của
    bất kỳ Controller nào** ở Site/AdministrativeUnit/Booking, downstream vẫn nhận đúng path cũ
    (VD `/api/role`) như trước giờ.
  - **Kết quả cuối**: `site-cluster` còn 1 route (`site-route`, catch-all, `Order: 100`) + 2 route
    riêng `site-module-my-route`/`site-module-toggle-route` (SiteModuleController cố tình chỉ lộ
    2/4 action qua Gateway) + `site-swagger-route`. `administrativeunit-cluster` và `booking-cluster`
    mỗi cluster còn đúng 2 route (`-read-route` GET anonymous, `-write-route` POST/PUT/DELETE
    UserOrAdminPolicy, áp dụng cho MỌI entity của cluster đó, không riêng từng entity nữa) + 1
    swagger route. Tổng cộng từ 16 route (13 route thật + 3 swagger) xuống còn 10 (7 route thật + 3
    swagger) — và quan trọng nhất: **entity mới thêm vào bất kỳ service nào theo đúng pattern phân
    quyền sẵn có của service đó (Site: uniform auth; AdministrativeUnit/Booking: đọc công khai/ghi
    cần auth) sẽ tự động hoạt động, không cần sửa `Gateway/appsettings.json` nữa**.
  - **Đã cập nhật theo**: `CommonLibs/BaseSQL/multi-tenant/tests/test_two_site_isolation.sh` — 12
    URL gọi qua `$GATEWAY_URL` tới Site service (`user-role`, `role`, `role-function`, `permission`,
    `site-module/toggle`) đổi thêm tiền tố `/api/site/...` (2 URL gọi thẳng `$SITE_URL` nội bộ và 2
    URL của Gateway tự host `/api/authorization/login`, `/api/user` giữ nguyên, không bị ảnh hưởng).
  - **Chưa cập nhật (biết trước, chưa làm)**: `docs/quan_li_thue_san/phase_1/FSR.md`/`Diagrams.md`
    (tài liệu hình thức đã xuất PDF, liệt kê endpoint kiểu `POST /api/field-type` — giờ thiếu tiền
    tố `/api/booking/`) và bảng route trong `multi-tenant/README.md` mục "Code C# đã viết" (vẫn mô
    tả đúng route CỦA CONTROLLER, chỉ thiếu tiền tố Gateway-facing mới) — không sửa vì ngoài phạm
    vi yêu cầu (chỉnh YARP), cần làm riêng nếu muốn tài liệu khớp 100%.
  - **Rủi ro lớn nhất cần verify khi có môi trường test thật**: route catch-all/2-route-đọc-ghi
    mới có thể vô tình match sai thứ tự nếu hiểu sai precedence của YARP — đã tra cứu doc xác nhận
    route path cụ thể (literal) luôn thắng `{**catch-all}` theo mặc định, và đã khai `Order: 100`
    tường minh cho `site-route` làm lớp bảo vệ thứ 2 — nhưng **chưa kiểm chứng bằng request thật**.
    Checklist test bắt buộc trước khi coi là xong: (1) toàn bộ API cũ (nay có tiền tố mới) của cả
    3 service vẫn gọi được qua Gateway đúng path mới; (2) `GET /api/site/site-module/my`,
    `PUT /api/site/site-module/toggle` vẫn hoạt động; (3) **quan trọng nhất** — xác nhận
    `POST /api/site-module/provision`/`GET /api/site-module/site/{siteId}` (2 action nội bộ, path
    KHÔNG có tiền tố `/api/site/`) **vẫn KHÔNG** gọi được qua Gateway dưới bất kỳ path nào — nếu lộ
    ra được là regression bảo mật, phải revert ngay; (4) chạy lại
    `test_two_site_isolation.sh` (đã cập nhật path) xác nhận PASS toàn bộ; (5) Swagger UI qua Gateway
    (`/site-swagger`, `/administrativeunit-swagger`, `/booking-swagger`) không bị ảnh hưởng (path
    riêng, không đụng `/api/` prefix mới).
  - `dotnet build BaseRepo.slnx`: 0 Error. JSON đã kiểm tra hợp lệ bằng cách strip comment `//`
    rồi parse thử (repo dùng comment `//` trong mọi `appsettings.json`, ASP.NET Core config
    provider của repo đã dung nạp việc này từ trước, không phải điểm mới).
- **Xoá `CommonLibs/Consul/` — dead code chưa từng được nối dây** (2026-09-01). Rà lại theo câu hỏi
  "Consul có còn cần dùng không" — grep toàn repo xác nhận `ConsulExtensions`/`ConsulHostedService`/
  `AddConsulRegistration` **không có nơi nào gọi tới**, kể cả `Program.cs` của cả 4 service. YARP
  Gateway route bằng địa chỉ tĩnh viết cứng trong `appsettings.json`, không dùng Consul để discovery.
  Service discovery thật sự (nếu bật) đi qua cơ chế khác đã có sẵn: Redis (key `Local_Url:{ApplicationId}`)
  + RabbitMQ (sự kiện `system.service.started/heartbeat/stopped`, xem `AddServiceDiscovery`/
  `ServiceDiscoveryHostedService` trong `CommonLibs/Services/ServiceCollectionExtensions.cs`) — hiện
  chỉ thực sự bật ở Booking (3 service kia đang comment `RabbitMqConnectionString`).
  - Xoá: `CommonLibs/Consul/` (3 file), `PackageReference Consul` trong `CommonLibs.csproj`, 2 dòng
    `using` thừa (`CommonLibs.Consul`, `Consul`) trong `ServiceCollectionExtensions.cs`, section
    `"Consul": {...}` trong `appsettings.json` của cả 4 service.
  - Cập nhật `claude.md` (bảng công nghệ chính, cây thư mục) bỏ mọi nhắc tới Consul như 1 công nghệ
    đang dùng.
  - `dotnet build BaseRepo.slnx`: 0 Error.
- **Tái cấu trúc thư mục repo theo nhóm domain** (2026-09-01) — gom `Gateway`/`Site`/`AdministrativeUnit` vào
  `Base/`, `Booking` vào `SportsBooking/`, thêm `BloodDonation/` làm chỗ chứa domain kế tiếp. Không đổi tên
  project/namespace nào, chỉ dời vị trí vật lý (`git mv`, giữ lịch sử) + sửa `ProjectReference` trỏ
  `CommonLibs` (thêm 1 cấp `..\`) + `BaseRepo.slnx` + `.vscode/launch.json`. Spec:
  `docs/superpowers/specs/2026-09-01-repo-folder-reorg-design.md`, plan:
  `docs/superpowers/plans/2026-09-01-repo-folder-reorg-plan.md`. `dotnet build BaseRepo.slnx`: 0 Error.

- **Microservice `Booking` — "Quản lý thuê sân thể thao", mới chỉ xong Phase 1 Foundation (chưa hết Phase 1)**
  (2026-09-01). Brainstorm ra spec đầy đủ trước
  (`docs/superpowers/specs/2026-09-01-booking-phase1-design.md`), nhưng plan triển khai đầu tiên
  (`docs/superpowers/plans/2026-09-01-booking-phase1-foundation-plan.md`) **cố tình chỉ làm phần nền** —
  không phải trọn Phase 1. Cũng đã xuất bộ tài liệu chính thức (BRD/FSR/SRS + DFD Mức 0/1/2) vào
  `docs/quan_li_thue_san/phase_1/`.
  - **Đã làm (build + commit + merge `master`)**: 5 project chuẩn `Booking.{Api,Application,Contract,Domain,
    Infrastructure}` (port 8003), CRUD đầy đủ cho `FieldType`/`Field`/`PricingRule` — `Field`/`PricingRule`
    Site-scoped đúng pattern bảo mật đã có của repo (SiteId luôn lấy từ claim `site_id`, không tin client input;
    `PricingRule` kiểm tra quyền sở hữu qua `Field` cha, đã fix 1 lỗi IDOR cross-Site lúc review). Wiring qua
    Gateway (route public-read/staff-write tách riêng, cluster, swagger). Script `CommonLibs/BaseSQL/booking/
    BaseBooking.sql` đã viết xong.
  - **Chưa làm (phần còn lại của Phase 1 theo spec mục 3.1/4/5/7 — chưa có plan triển khai)**:
    1. Entity `Customer` (khách đặt sân) — đăng ký/đăng nhập Email/SĐT + mật khẩu, `CustomerAuthController` dùng
       JWT scheme riêng (khác Bearer của staff, không đi qua `AuthorizationMiddleware`/RBAC hiện có),
       `CustomerPolicy` check claim `CustomerId`.
    2. Entity `Booking` (bản ghi đặt sân) — `BookingStatus` (Pending/Confirmed/CheckedIn/Completed/Cancelled/
       NoShow), `PaymentStatus` (Unpaid/Deposited/Paid, Phase 1 chỉ đánh dấu thủ công, chưa tích hợp cổng thanh
       toán online).
    3. Entity `RecurringGroup` (đặt sân định kỳ) — sinh nhiều `Booking` từ 1 nhóm.
    4. **Logic chống trùng lịch** — nghiệp vụ quan trọng nhất Phase 1 theo spec ("không phải CRUD thuần"), áp
       dụng cho cả đặt lẻ lẫn từng bản ghi sinh từ `RecurringGroup`.
    5. Web nội bộ cho Lễ tân/Admin quản lý `Booking`/`Customer` (huỷ, sửa, đánh dấu thu tiền mặt thủ công).
  - **Vận hành còn tồn đọng** (dù đã có phần Foundation): (a) chưa chạy `BaseBooking.sql` lên SQL Server thật;
    (b) chưa seed các `Function` khớp tên action (`CreateFieldType`, `UpdateField`, `DeleteFieldType`,
    `CreatePricingRule`... đúng quy ước `[RequireFunction]` đã dùng cho Site/AdministrativeUnit) + gán qua
    `Role_Function`, nếu không API mutating của Booking sẽ luôn 403.
  - Ngoài phạm vi Phase 1 (nhắc lại từ spec, để lại Phase 2/3): POS bán hàng hoá, tích hợp cổng thanh toán online
    (VNPay/MoMo/ZaloPay), thống kê/Power BI.
- **Tách `SensitiveHeaderMasker` dùng chung + fix lỗ hổng log token/cookie thô ở `GlobalExceptionMiddleware`**
  (2026-09-01, cùng ngày, ngay sau việc thêm SQL logging phía dưới). Phát hiện qua brainstorm: `SerilogExtension.cs`
  và `Log4netExtension.cs` đều có sẵn 1 bản riêng của mảng `SensitiveHeaders` + hàm `MaskSensitiveValue` (trùng lặp
  y hệt nhau), nhưng `CommonLibs/Middlewares/GlobalExceptionMiddleware.cs` (log `RequestHeaders` mỗi khi có
  exception, cả business lẫn fatal) lại log **`h.Value.ToString()` thô, không mask** — nghĩa là header
  `Authorization`/`Cookie` bị ghi plaintext ra log bất cứ khi nào request đó rơi vào exception. Fix theo đúng
  hướng repo `viettin.bloodstore.back` từng áp dụng khi gặp bug tương tự:
  - Tạo `CommonLibs/Logging/SensitiveHeaderMasker.cs` (public static, hàm `Mask(key, value)` + mảng
    `SensitiveHeaders`: `Authorization`, `Cookie`, `Set-Cookie`, `Password`, `RefreshToken`, `AccessToken`) làm
    nguồn sự thật duy nhất.
  - `SerilogExtension.cs`/`Log4netExtension.cs`: xóa bản `SensitiveHeaders`/`MaskSensitiveValue` riêng, đổi toàn
    bộ chỗ gọi (request + response headers) sang `SensitiveHeaderMasker.Mask(...)`.
  - `GlobalExceptionMiddleware.cs`: áp `SensitiveHeaderMasker.Mask(...)` cho `requestHeaders` trước khi log —
    đây là fix chính, trước đó hoàn toàn không mask.
  - `dotnet build BaseRepo.slnx`: 0 Error.
  - **Chưa test lại end-to-end** — cần gọi 1 request có header `Authorization` mà rơi vào exception (VD token
    sai/hết hạn) ở ít nhất 1 service, xác nhận log không còn hiện token/cookie plaintext.
- **Thêm SQL logging (mask tham số nhạy cảm) vào `BaseDbContext`, áp dụng cho toàn bộ hàm Dapper** (2026-09-01).
  Tham khảo cách xử lý che dữ liệu nhạy cảm + ghi log SQL đã có sẵn ở repo `viettin.bloodstore.back`
  (`OneLIS.API.BloodStorage.CommonLibs/Logging/SqlLoggingHelper.cs` +
  `.../Middlewares/GlobalExceptionMiddleware.cs`), port sang `CommonLibs/BaseDbContext/BaseDbContext.cs`:
  - Tạo `CommonLibs/Logging/SqlLoggingHelper.cs` (internal static) — che tham số SQL theo tên (chứa
    `password`/`token`/`secret`/`authorization`/`connectionstring`/`apikey`, không phân biệt hoa thường) trước khi
    log, đồng thời build sẵn script `DECLARE @Param TYPE = value;` từ tham số đã che để tiện copy vào SSMS debug
    lại câu SQL. **Cố tình không port** overload `GetSafeParameters(IEnumerable<DbParameter>?)` và các hàm
    multi-connection (`QueryOneLisAsync`, `ISqlConnectionFactory`) của repo nguồn — BaseRepo hiện không có nhu cầu
    multi-DB tương tự, tránh thêm code chết.
  - Thêm hàm `ExecuteLoggedAsync<TResult>` (private) vào `BaseContext` — log `SQL {Operation} executing` trước khi
    chạy, `completed in {ElapsedMilliseconds}ms` khi thành công, `failed after {ElapsedMilliseconds}ms` kèm
    exception khi lỗi (dùng `_logger` đã có sẵn trong `BaseContext`, format log giữ nguyên như thiết kế gốc).
  - Áp dụng `ExecuteLoggedAsync` cho cả 4 hàm Dapper (`QueryAsync`, `QueryFirstOrDefaultAsync`,
    `ExecuteScalarAsync`, `ExecuteAsync`) — bỏ khối `try { } catch { throw; }` thừa cũ (không có tác dụng, giờ
    logging + rethrow đã nằm trong `ExecuteLoggedAsync`).
  - `dotnet build BaseRepo.slnx`: 0 Error.
  - **Chưa test lại end-to-end** — vì đây là thay đổi trong `BaseRepository`/mọi truy vấn Dapper toàn hệ thống,
    cần chạy thử ít nhất 1 API mỗi service (Gateway/Site/AdministrativeUnit/Booking) và kiểm tra log xuất hiện
    đúng format `SQL {Operation} executing/completed/failed` trước khi coi là xong.
- **Nâng toàn bộ solution từ .NET 6 lên .NET 10** (2026-09-01, cùng ngày, ngay sau đợt tái cấu trúc 5-layer phía
  dưới). Đổi `<TargetFramework>net6.0</TargetFramework>` → `net10.0` cho 16/17 project (`net6.0-windows` →
  `net10.0-windows` cho `SystemManagement`, project thứ 17) bằng thay thế hàng loạt, không đổi logic.
  - **Bump package đi kèm để khớp net10** (dùng version mới nhất trên NuGet tại thời điểm nâng cấp):
    `Microsoft.EntityFrameworkCore`/`.SqlServer` 6.0.36 → `10.0.11`, `Microsoft.AspNetCore.Authentication.JwtBearer`
    6.0.36 → `10.0.11`, `Microsoft.Extensions.Caching.StackExchangeRedis` 6.0.36 → `10.0.11`,
    `Microsoft.Data.SqlClient` 5.1.5 → `7.0.2`, `Yarp.ReverseProxy` 1.1.2 → `2.3.0`, `Swashbuckle.AspNetCore` 6.5.0
    → `10.2.3`. `Microsoft.Web.Administration` (11.1.0) và `Microsoft.Extensions.Logging.Log4Net.AspNetCore`
    (10.0.0) đã sẵn ở bản mới nhất, không đổi.
  - **Gỡ `PackageReference Ocelot`** khỏi `Gateway.Api.csproj` — chỉ tồn tại trong `.csproj`, không được dùng ở bất
    kỳ đâu trong code (Gateway thực tế dùng YARP), và version cũ (18.0.0) là rủi ro chặn nâng cấp không cần thiết.
  - **3 lỗi build thật sự phát sinh do breaking change của .NET 10 / package mới (không phải do đổi TFM đơn
    thuần), đã fix, không đổi hành vi nghiệp vụ:**
    1. `CommonLibs/Utils/RedisHelper.cs` + `CommonLibs/Services/RedisService.cs` — `JsonSerializer.Deserialize<T>(value!)`
       (với `value` kiểu `RedisValue`) bị lỗi `CS0121` ambiguous giữa overload `Deserialize(string)` và
       `Deserialize(ReadOnlySpan<byte>)` (overload span mới của .NET 10) vì `RedisValue` implicit convert được cả
       2 kiểu. Fix: ép kiểu tường minh `(string)value!` để giữ đúng hành vi cũ (dữ liệu vốn lưu dạng string qua
       `StringSet`/`StringSetAsync`).
    2. `Gateway.Infrastructure/Repositories/UserRepository.cs` — `using Microsoft.Identity.Client;` (import
       không dùng tới trong file, chỉ tồn tại nhờ ăn theo dependency bắc cầu cũ của `Microsoft.Data.SqlClient`
       5.1.5) bị mất namespace sau khi bump `Microsoft.Data.SqlClient` lên 7.0.2 (dependency bắc cầu đổi khác).
       Xoá import thừa.
    3. **Swashbuckle.AspNetCore 10.x kéo theo Microsoft.OpenApi 2.x, đổi namespace/API đáng kể** — ảnh hưởng cả 3
       `{Service}.Api`: `Program.cs` (cấu hình Swagger) và `Utils/AddRequiredHeaderParameter.cs` (OperationFilter
       thêm header `X-Device-ID`/`X-Device-Name`):
       - `using Microsoft.OpenApi.Models;` → `using Microsoft.OpenApi;` (toàn bộ type như `OpenApiOperation`,
         `OpenApiParameter`, `OpenApiSchema`, `OpenApiInfo`, `OpenApiSecurityScheme`... chuyển thẳng lên namespace
         gốc `Microsoft.OpenApi`).
       - `Microsoft.OpenApi.Any.OpenApiString` (và họ `IOpenApiAny`) bị xoá hoàn toàn, thay bằng
         `System.Text.Json.Nodes.JsonNode`/`JsonValue.Create(...)`.
       - `operation.Parameters` đổi kiểu từ `IList<OpenApiParameter>` sang `IList<IOpenApiParameter>` (interface-
         based) → `??= new List<OpenApiParameter>()` phải đổi thành `new List<IOpenApiParameter>()`.
       - `OpenApiSchema.Type` đổi từ `string` sang enum `JsonSchemaType?` → `Type = "string"` phải đổi thành
         `Type = JsonSchemaType.String`.
       - `AddSecurityRequirement` đổi hẳn cách khai báo Bearer security requirement: pattern cũ dùng
         `new OpenApiSecurityScheme { Reference = new OpenApiReference {...} }` làm key trong
         `OpenApiSecurityRequirement` không còn hợp lệ (`OpenApiSecurityScheme` không còn property `Reference`,
         `OpenApiReference` bị xoá) — đổi sang overload nhận delegate
         `options.AddSecurityRequirement(document => new OpenApiSecurityRequirement { [new
         OpenApiSecuritySchemeReference("Bearer", document)] = [] })`.
  - **SystemManagement (WinForms) — brainstorm riêng vì có 2 lựa chọn xung đột nhau:** ban đầu định "bỏ qua"
    (giữ `net6.0-windows`) vì lo `DevExpress.Data 23.2.15` chưa hỗ trợ net10, nhưng việc này khiến
    `ProjectReference` tới `CommonLibs` (đã lên net10) gãy ngay ở bước restore (`NU1201`) — kiểm tra kỹ thì
    `SystemManagement` **có dùng thật** `EnvironmentHelper`/`Cypher.AesCypher`/`Cypher.CypherHelper`/
    `Cypher.RsaCypher`/`FileHelper` từ `CommonLibs.Utils` (không phải reference thừa như Ocelot), nên không thể
    gỡ `ProjectReference` mà không gãy 2 file (`UC_ConfigEnv.cs`, `FrmMain.cs`). Đã hỏi lại và người dùng chọn
    nâng luôn `SystemManagement` lên `net10.0-windows` — build thử xác nhận **`DevExpress.Data 23.2.15` thực ra
    build được bình thường trên net10** (không phải rủi ro thật như lo ban đầu, có thể do package này target
    netstandard/đa-target thay vì ghim cứng net6).
    - Phát sinh thêm 1 lỗi build khác do **analyzer mới của WinForms trong .NET 10 SDK (`WFO1000`)**: property
      public trên `UserControl` (`BaseUC`, `Title` — cả 2 thuộc interface `IBasePage`) không khai báo ý định
      serialize cho Designer sẽ bị chặn build (đổi từ warning → error kể từ .NET 9+, mục đích chống rò rỉ dữ
      liệu private vào code Designer serialize tự động). Fix bằng cách thêm
      `[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]` lên `BaseUC`/`Title` ở cả
      `Views/Home/UC_Home.cs` và `Views/ConfigEnv/UC_ConfigEnv.cs` (2 property này chỉ dùng runtime, không cần
      Designer serialize).
  - **Tiện thể sửa `.vscode/launch.json`** — file này đã lệch từ đợt tái cấu trúc 5-layer 2026-09-01 (còn trỏ
    path phẳng cũ `Gateway/bin/Debug/net6.0/Gateway.dll`, `Site/bin/Debug/net6.0/Site.dll`, sai cả tên file lẫn
    cấu trúc thư mục), giờ cập nhật đúng path `{Service}/{Service}.Api/bin/Debug/net10.0/{Service}.Api.dll` cho
    cả 3 service (thêm luôn cấu hình debug cho `AdministrativeUnit` — trước đó launch.json chưa có, chỉ có
    Gateway/Site).
  - `dotnet restore` + `dotnet build BaseRepo.slnx` (toàn bộ 17 project, kể cả `SystemManagement`): **0 Error**
    (146 warning, toàn bộ là `NU1903` cảnh báo lỗ hổng bảo mật đã có từ trước ở `AutoMapper 13.0.1` và
    `System.Security.Cryptography.Xml 8.0.2` — transitive dependency, không phát sinh do đổi TFM, **chưa xử lý**
    vì ngoài phạm vi việc nâng cấp lần này) + warning nullable có sẵn từ trước.
  - **Chưa test runtime end-to-end** (chỉ mới xác nhận build) — cần `dotnet run` thử cả 3 service + kiểm tra kỹ
    `SystemManagement` (đặc biệt luồng liên quan `Cypher.*`/`EncryptionLibrary` vì đây là lần đầu chạy trên
    net10, và cấu hình `PublishSingleFile`/`SelfContained` khi publish thật) trước khi coi là xong.

- **Áp dụng lại cấu trúc 5-layer (như Gateway) cho `Site` và `AdministrativeUnit` + tách Serilog config cho cả 3
  service (2026-09-01, cùng ngày, ngay sau pilot Gateway).** Lặp lại chính xác quy trình đã làm cho Gateway:
  - **Site** (8 entity: Site, Module, Role, Screen, Function, RoleFunction, UserRole, SiteModule — nhiều hơn dự
    kiến ban đầu, `SiteModule` được thêm sau lần khảo sát đầu của Gateway pilot): gom `Site`/`Site.Application`/
    `Site.Domain`/`Site.Infrastructure` vào folder cha `Site/`, đổi Presentation `Site` → `Site.Api` (namespace
    `Site.Controllers`/`Site.Utils` → `Site.Api.Controllers`/`Site.Api.Utils`), tạo `Site.Contract` chứa 9 file
    DTO (`FunctionDTO`, `ModuleDTO`, `PermissionDTO`, `RoleDTO`, `RoleFunctionDTO`, `ScreenDTO`, `SiteDTO`,
    `SiteModuleDTO`, `UserRoleDTO` — `PermissionDTO` không có Entity tương ứng vì Permission là API tổng hợp,
    không phải bảng riêng). Sửa toàn bộ `using Site.Domain.DTOs;` → `using Site.Contract.DTOs;` bằng `sed` hàng
    loạt (không sót file nào, xác nhận bằng grep = 0 sau khi chạy).
  - **AdministrativeUnit** (2 entity: Province, Ward): làm y hệt, tạo `AdministrativeUnit.Contract` chứa
    `ProvinceDTO`/`WardDTO`, Presentation `AdministrativeUnit` → `AdministrativeUnit.Api`.
  - Cả 2 service: cập nhật relative path `ProjectReference`/`BaseRepo.slnx` theo đúng công thức đã dùng cho
    Gateway (CommonLibs từ `..\CommonLibs` → `..\..\CommonLibs` do lồng thêm 1 cấp; thêm `ProjectReference` tới
    `{Service}.Contract` cho `.Api`/`.Application`/`.Infrastructure`).
  - **Tách Serilog cho cả 3 service** (Gateway đã làm trước đó cùng ngày, giờ làm tiếp Site/AdministrativeUnit):
    tạo `{Service}.Api/appsettings.Serilog.json` chứa nguyên section `"Serilog"` (giữ nguyên comment), xoá khỏi
    `appsettings.json`, thêm `builder.Configuration.AddJsonFile("appsettings.Serilog.json", optional: false,
    reloadOnChange: true);` trong `Program.cs` trước `builder.AddDynamicLogging()` — bắt buộc vì ASP.NET Core
    không tự nạp file `appsettings.*.json` ngoài quy ước `appsettings.json`/`appsettings.{Environment}.json`.
  - `dotnet build BaseRepo.slnx` (toàn bộ solution, cả 3 service): **0 Error**. Đã xác nhận cả 2 file
    `appsettings.Serilog.json` (Site, AdministrativeUnit) tự được copy vào `bin/Debug/net6.0/` sau build, cùng cơ
    chế `appsettings.*.json` glob như Gateway.
  - **Kết quả: không còn trạng thái "pilot" nữa** — cả 3 service (`Gateway`, `Site`, `AdministrativeUnit`) giờ
    đồng nhất 1 khuôn 5-layer (`.Api`/`.Application`/`.Contract`/`.Domain`/`.Infrastructure`, gom vào 1 folder cha
    mỗi service) + Serilog config tách riêng. Đã đơn giản hoá lại toàn bộ tài liệu liên quan (`claude.md` phần
    kiến trúc/naming convention, `.claude/rules/01-architecture.md`, `.claude/skills/create-*`) — bỏ hết các đoạn
    điều kiện "nếu đã tách/chưa tách" vì giờ là quy ước chung không còn ngoại lệ. `SystemManagement`/`Elastic`
    không thuộc kiến trúc 5-layer này (không đổi).
  - **Chưa test runtime end-to-end** cho Site/AdministrativeUnit (chỉ mới xác nhận build) — nên `dotnet run` thử
    cả 2 service trước khi coi là xong, tương tự khuyến nghị đã ghi cho Gateway ở mục dưới.

- **Tái cấu trúc thư mục Gateway (pilot) + tách Contract layer khỏi Domain (2026-09-01).** Gom 4 project cũ
  (`Gateway`, `Gateway.Application`, `Gateway.Domain`, `Gateway.Infrastructure` — trước đó nằm flat ở root
  `BaseRepo/`) vào 1 folder cha `Gateway/` bằng `git mv` (giữ lịch sử), **không đổi tên project/namespace** — chỉ
  đổi vị trí vật lý, nên không phải sửa bất kỳ `using` nào liên quan tới các namespace đó. Tạo project mới
  `Gateway/Gateway.Contract/Gateway.Contract.csproj`, chuyển 2 file DTO (`AuthorizationDTO.cs`, `UserDTO.cs`, tổng
  cộng 8 class Input/Output/Filter) từ `Gateway.Domain/DTOs/` sang, đổi namespace `Gateway.Domain.DTOs` →
  `Gateway.Contract.DTOs`. Domain giờ chỉ còn `Entities/User.cs`.
  - Sửa 25 file (`using Gateway.Domain.DTOs;` → `using Gateway.Contract.DTOs;`) trải khắp Controllers/
    Repositories/Interfaces/Features/Mappings — `using Gateway.Domain.Entities;` (4 chỗ) giữ nguyên, Entity
    không di chuyển.
  - Cập nhật relative path `ProjectReference`/`HintPath` trong 4 csproj cũ do đổi độ sâu thư mục (CommonLibs từ
    `..\CommonLibs` → `..\..\CommonLibs` vì Gateway giờ lồng thêm 1 cấp), thêm `ProjectReference` tới
    `Gateway.Contract` cho `Gateway`/`Gateway.Application`/`Gateway.Infrastructure` (3 project đang dùng DTO).
    Cập nhật `BaseRepo.slnx` (path 5 project Gateway.*).
  - **CommonLibs giữ nguyên hoàn toàn** (tên folder/project/namespace) — không đổi thành `Common.Libs` như thảo
    luận ban đầu, theo yêu cầu chốt lại của người dùng.
  - `dotnet build BaseRepo.slnx`: 0 Error (252 warning, toàn bộ là warning nullable/CS8601 có sẵn từ trước, không
    phát sinh mới).
  - **Phạm vi cố ý chỉ giới hạn Gateway** (pilot) — `Site`/`AdministrativeUnit` **chưa** áp dụng cấu trúc
    folder-theo-service hay tách Contract, vẫn giữ nguyên flat + DTO trong Domain. Nếu pilot này ổn, làm tiếp
    tương tự cho `Site` (8 entity) rồi `AdministrativeUnit` (2 entity) ở đợt sau.
  - Cập nhật tài liệu đi kèm: `claude.md` (cây thư mục, Dependency Rule, Naming Convention), `.claude/rules/
    01-architecture.md`, `.claude/rules/05-checklist.md`; tách skill `create-domain-layer` (bỏ phần DTO) + skill
    mới `create-contract-layer` (phần DTO, chỉ áp dụng cho service đã tách); cập nhật `create-api-feature`,
    `create-application-layer`, `create-infrastructure-layer`, `create-presentation-layer` (namespace DTO trong
    template đổi theo service đã/chưa tách Contract).
  - **Chưa test runtime end-to-end** (chỉ mới xác nhận build) — di chuyển thư mục không đổi output `bin/`, nhưng
    nên chạy thử `dotnet run` Gateway 1 lần trước khi coi là xong, đặc biệt path tương đối trong
    `Gateway/Gateway.Api/Keys/` (JWT signing keys) và `log4net.config` không đổi vì nằm cùng cấp project như cũ.
  - **Follow-up cùng ngày: đổi tên project Presentation `Gateway` → `Gateway.Api`** (`git mv` folder + rename
    `Gateway.csproj` → `Gateway.Api.csproj`), theo yêu cầu người dùng để phân biệt rõ project Presentation với
    tên folder cha `Gateway/` (tránh 2 cấp thư mục trùng tên `Gateway/Gateway/`). Đổi namespace
    `Gateway.Controllers`/`Gateway.Utils` → `Gateway.Api.Controllers`/`Gateway.Api.Utils` trong 4 file .cs của
    project này (`using Gateway.Utils;` trong `Program.cs` cũng đổi theo) — không ảnh hưởng namespace của
    `Gateway.Application`/`Gateway.Contract`/`Gateway.Domain`/`Gateway.Infrastructure` (giữ nguyên, đây là 4
    project riêng biệt không đổi tên). Cập nhật `BaseRepo.slnx`. `dotnet build`: 0 Error. **Quy ước từ nay:**
    service đã áp dụng pilot có Presentation layer tên `{ServiceName}.Api` (không phải bare `{ServiceName}`
    nữa); service chưa áp dụng (Site, AdministrativeUnit) vẫn dùng tên bare `{ServiceName}` như cũ.

- **Thêm microservice mới `AdministrativeUnit`** (Province + Ward) — tách riêng thành 1 module đầy đủ 4 layer
  giống `Site`/`Gateway`, theo checklist chuẩn (`05-checklist.md`) + skill `create-api-feature`. Dựa trên schema
  có sẵn `CommonLibs/BaseSQL/administrative-unit/BaseAdministrativeUnit.sql` (2 bảng `Province`/`Ward`, không có
  `tbl_` prefix, PK là `Code` chứ không phải `AutoId` như mọi entity khác) + pipeline seed dữ liệu đã có sẵn từ
  trước (`01_migrate_schema.sql` → `05_cleanup_staging.sql`, seed ~34 Province/3321 Ward từ nguồn mở theo Nghị
  quyết 202/2025/QH15). Đã brainstorm và chốt 3 quyết định thiết kế quan trọng trước khi code:
  1. **Phạm vi API**: đầy đủ CRUD + Import/Export cho cả Province và Ward (không phải chỉ đọc), giống hệt pattern
     `Module`/`Role`/`Screen`... Thêm 1 API tiện ích `GET /api/ward/by-province/{provinceCode}`
     (`GetWardsByProvince`, giống `GetFunctionsByRole`) để lấy danh sách Ward theo Province.
  2. **Authorization**: vì đây là dữ liệu tham chiếu công khai (tên tỉnh/phường dùng cho dropdown địa chỉ), các
     action đọc (`GetById`/`GetAll`/`Export`/`GetWardsByProvince`) **cố tình không gắn `[Authorize]`** (anonymous)
     — khác với mọi controller khác trong repo (luôn class-level `UserOrAdminPolicy`). Action mutating
     (Create/Update/Delete/Import) vẫn gắn `[Authorize(Policy="UserOrAdminPolicy")]` + `[RequireFunction("...")]`
     như bình thường. Hệ quả: `Gateway/appsettings.json` phải tách **2 route YARP riêng theo Methods** cho mỗi
     entity (`province-read-route`/`ward-read-route` chỉ `GET`, không `AuthorizationPolicy` ; `province-write-route`/
     `ward-write-route` chỉ `POST/PUT/DELETE`, có `AuthorizationPolicy: UserOrAdminPolicy`) — vì
     `AuthorizationMiddleware` bỏ qua toàn bộ xác thực khi route không có `IAuthorizeData` metadata, nếu gộp
     chung 1 route như các entity khác thì Gateway sẽ chặn nhầm cả action đọc.
  3. **Database riêng**: service mới kết nối DB SQL Server riêng tên `AdministrativeUnit` (không dùng chung
     `BaseRepo` với Gateway/Site) — đúng theo `USE AdministrativeUnit` đã có sẵn trong script. Tạm dùng chung
     UserName/Password đã mã hoá với Site (cùng SQL Server instance `localhost,1433`) — **cần xác nhận lại** nếu
     login DB thực tế khác.
  - **Vá lệch convention AutoId (đã sửa lại lần 2 theo yêu cầu)**: ban đầu schema gốc không có cột `AutoId`
    (chỉ PK `Code`), khác mọi bảng khác trong `BaseSQL.sql` (luôn `AutoId BIGINT IDENTITY(1,1) PRIMARY KEY`,
    business key chỉ là `UNIQUE`). Lần đầu đã vá tạm bằng 1 migration ALTER TABLE riêng (thêm `AutoId` như cột
    thường, giữ `Code` làm PK) — sau đó theo yêu cầu, đã **sửa thẳng vào `BaseAdministrativeUnit.sql`** cho đúng
    100% convention `BaseSQL.sql`: `Province`/`Ward` giờ có `[AutoId] BIGINT IDENTITY(1,1)` làm
    `PRIMARY KEY CLUSTERED`, `Code` đổi thành `CONSTRAINT UQ_..._Code UNIQUE` (không còn là PK). Đã xoá file
    migration ALTER TABLE cũ (không cần nữa vì AutoId giờ nằm sẵn trong script tạo bảng gốc). Không cần sửa gì
    ở tầng code C# (Entity/Repository vẫn `Select *`/lọc theo `Code` như cũ, không quan tâm cột nào là PK).
  - Port **8002**, Consul `administrativeunit-service`. Đã thêm 4 project mới vào `BaseRepo.slnx`
    (`AdministrativeUnit`, `.Application`, `.Domain`, `.Infrastructure`), route + swagger doc tại Gateway.
  - Đã review bằng code-reviewer subagent (đối chiếu SQL/CQRS/DI/route/YARP config với pattern `Module`/`Site`
    có sẵn) — không phát hiện bug, chỉ thiếu 2 file template import mẫu (`Import_Provinces_mau.xlsx`,
    `Import_Wards_mau.xlsx`, không ảnh hưởng runtime, chỉ là tài liệu mẫu). `dotnet build BaseRepo.slnx` thành
    công, 0 Error.
  - **Chưa test end-to-end** (người dùng chủ động test sau, cần xác nhận theo `06-testing.md` trước khi test).

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

- **Đổi URL scheme YARP theo tên service (mục mới nhất phía trên) — CHƯA TEST, bắt buộc test khi
  có máy đủ SQL Server/Redis**. Checklist đã ghi chi tiết ở mục "Đã hoàn thành" tương ứng — quan
  trọng nhất là xác nhận 2 action nội bộ `SiteModuleController` (`ProvisionSiteModule`,
  `GetSiteModulesBySite`) **vẫn không** lộ ra qua Gateway dưới bất kỳ path nào sau khi đổi scheme.
  Đồng thời **mọi URL cũ đã đổi** (`/api/ward` → `/api/administrativeunit/ward`, `/api/field` →
  `/api/booking/field`, `/api/role` → `/api/site/role`...) — nếu có FE/Postman collection/tài
  liệu nào khác đang gọi URL cũ ngoài phạm vi đã rà (`test_two_site_isolation.sh` đã sửa), cần cập
  nhật thêm. `docs/quan_li_thue_san/phase_1/FSR.md`/`Diagrams.md` và bảng route trong
  `multi-tenant/README.md` còn liệt kê path cũ (chưa có tiền tố mới) — cố tình chưa sửa, để riêng
  nếu muốn tài liệu khớp 100% với behavior mới.
- **Booking Phase 1 chưa hoàn tất (mục mới nhất phía trên)** — mới xong phần Foundation
  (`FieldType`/`Field`/`PricingRule` CRUD). Trước khi làm tiếp cần: (1) chạy `BaseBooking.sql` lên DB thật + seed
  `Function`/`Role_Function` cho các action mutating hiện có (nếu không sẽ luôn 403 khi test); (2) brainstorm +
  viết plan triển khai riêng cho phần còn lại của Phase 1 (`Customer`, `Booking`, `RecurringGroup`, logic chống
  trùng lịch, web nội bộ cho Lễ tân/Admin) — spec đã có sẵn ở `docs/superpowers/specs/
  2026-09-01-booking-phase1-design.md`, chỉ còn thiếu plan.
- **`SensitiveHeaderMasker` + SQL logging (2 mục mới nhất phía trên) mới chỉ xác nhận build, chưa test runtime**:
  (1) gọi 1 request có header `Authorization` mà rơi vào exception (VD token sai/hết hạn) ở ít nhất 1 service,
  xác nhận log (Serilog/log4net/GlobalExceptionMiddleware) không còn hiện token/cookie plaintext; (2) chạy thử
  ít nhất 1 API dùng Dapper mỗi service (Gateway/Site/AdministrativeUnit/Booking), xác nhận log xuất hiện đúng
  format `SQL {Operation} executing/completed/failed` và tham số nhạy cảm (password/token/secret/...) bị che
  thành `***`.
- **Nâng cấp .NET 10 (mục mới nhất phía trên) mới chỉ xác nhận build, chưa test runtime** — cần `dotnet run`
  thử cả 3 service (Gateway, Site, AdministrativeUnit) và chạy thử `SystemManagement.exe` (đặc biệt màn hình
  `ConfigEnv` dùng `Cypher.*`/`EncryptionLibrary`) trước khi coi việc nâng cấp là hoàn tất. Đồng thời còn 2 cảnh
  báo bảo mật `NU1903` tồn đọng từ trước (không liên quan đến đợt nâng cấp này, cố tình chưa xử lý vì ngoài phạm
  vi task): `AutoMapper 13.0.1` và `System.Security.Cryptography.Xml 8.0.2` (transitive qua nhiều package) — cân
  nhắc bump riêng nếu cần khi có yêu cầu.
- **Microservice `AdministrativeUnit` (mục phía trên) chưa được test lại** — trước khi test cần:
  (1) chạy `BaseAdministrativeUnit.sql` (đã sửa lại: `AutoId` giờ là PRIMARY KEY, `Code` là UNIQUE) trên DB
  `AdministrativeUnit` — **nếu DB đã tồn tại từ trước theo schema PK `Code` cũ (đã lỡ chạy bản trước khi sửa),
  cần xoá/tạo lại bảng `Province`/`Ward`** vì đổi PK không thể ALTER an toàn khi bảng đã có dữ liệu/PK khác; nếu
  DB chưa từng tạo thì chạy thẳng bản mới là đủ, (2) chạy tiếp `01`-`05` để seed dữ liệu (34 Province/3321 Ward)
  nếu chưa có, (3) xác nhận UserName/Password kết nối DB trong `Base/AdministrativeUnit/AdministrativeUnit.Api/appsettings.json` đúng (đang
  tạm dùng chung với Site). Theo `06-testing.md`, cần hỏi xác nhận trước khi test và chỉ test riêng API này. Lưu
  ý API đọc (`GetById`/`GetAll`/`Export`/`GetWardsByProvince`) không cần token (anonymous), chỉ API mutating mới
  cần. Test account cần Function `CreateProvince`/`UpdateProvince`/`DeleteProvince`/`ImportProvinces` (và tương
  tự cho Ward) qua `Role_Function`, hoặc là Role literal "Admin", mới gọi được action mutating.
- Thiếu 2 file template import mẫu `Import_Provinces_mau.xlsx`/`Import_Wards_mau.xlsx` trong
  `CommonLibs/TemplatesImport/` (không ảnh hưởng runtime, chỉ thiếu tài liệu mẫu) — có thể bổ sung sau nếu cần.
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
- Route YARP `site-route` trong `Base/Gateway/Gateway.Api/appsettings.json` có `Match.Path: "api/site/{**catch-all}"` thiếu dấu
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
