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
