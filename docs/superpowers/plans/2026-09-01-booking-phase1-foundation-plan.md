# Booking Phase 1 Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stand up the new `Booking` microservice (5-layer, per BaseRepo convention) with Admin/Lễ tân-managed `FieldType`, `Field`, and `PricingRule` data, wired through the Gateway with public read routes.

**Architecture:** New service `Booking/{Booking.Api,.Application,.Contract,.Domain,.Infrastructure}` mirroring `AdministrativeUnit`. `Field` carries `SiteId` (tenant boundary, taken from the caller's `site_id` claim — never trusted from client input). Staff auth reuses Gateway's existing JWT/RBAC (`UserOrAdminPolicy` + `[RequireFunction]`) — no changes to Gateway/Site/AdministrativeUnit auth code. Read endpoints (browse field types/fields/prices) are public; write endpoints require staff auth.

**Tech Stack:** .NET 10, ASP.NET Core Web API, MediatR (CQRS), Dapper over `BaseRepository`/`BaseContext`, SQL Server, YARP (Gateway), AutoMapper.

**Spec:** [docs/superpowers/specs/2026-09-01-booking-phase1-design.md](../specs/2026-09-01-booking-phase1-design.md)

## Global Constraints

- Every API returns `Response<T>` (`Success`, `Code`, `Message`, `Data`) — see `CommonLibs/Utils/Response.cs`.
- Every business exception is `AppException` with message prefix `[{Entity}_Repo_{Method}-{Number}]`.
- Every entity extends `BaseEntity` (`CommonLibs.Entities`) — `BaseEntity.Status` is soft-delete only (`Active=1/Inactive=0/Deleted=-1`); any entity-specific business status (e.g. `Field.OperationalStatus`) is a **separate** property, never overloaded onto `BaseEntity.Status`.
- `SiteId` for staff-mutated resources is **always** taken from the caller's `site_id` claim (`BaseController.CurrentSiteId`) — never bound from client-supplied request body/query, per the existing `SiteModuleController` precedent.
- Target framework `net10.0` for all new projects, matching the rest of the solution.
- **Out of scope for this plan** (deliberately, per spec §8 and to keep this plan focused): `Customer` entity/auth, `Booking`/`RecurringGroup` entities and the conflict-check logic, Import/Export for `FieldType`/`Field`/`PricingRule` (spec never asked for bulk import of this data), POS/payment/Power BI. These land in a follow-up plan.

---

### Task 1: Scaffold the 5 Booking projects

**Files:**
- Create: `Booking/Booking.Domain/Booking.Domain.csproj`
- Create: `Booking/Booking.Contract/Booking.Contract.csproj`
- Create: `Booking/Booking.Application/Booking.Application.csproj`
- Create: `Booking/Booking.Infrastructure/Booking.Infrastructure.csproj`
- Create: `Booking/Booking.Api/Booking.Api.csproj`
- Create: `Booking/Booking.Api/Program.cs` (minimal placeholder — see Step 6.5; Task 2 Step 6 replaces its content)
- Modify: `BaseRepo.slnx`

**Interfaces:**
- Produces: 5 buildable projects that later tasks add files to (`Booking.Api` needs a placeholder `Program.cs` to be buildable at all — see Step 6.5).

- [ ] **Step 1: Create `Booking.Domain.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create `Booking.Contract.csproj`** (identical shape to Domain's)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Create `Booking.Application.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.5.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Booking.Contract\Booking.Contract.csproj" />
    <ProjectReference Include="..\Booking.Domain\Booking.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Folder Include="Features\" />
  </ItemGroup>

</Project>
```

- [ ] **Step 4: Create `Booking.Infrastructure.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Data.SqlClient" Version="7.0.2" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.11" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Booking.Application\Booking.Application.csproj" />
    <ProjectReference Include="..\Booking.Contract\Booking.Contract.csproj" />
    <ProjectReference Include="..\Booking.Domain\Booking.Domain.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 5: Create `Booking.Api.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <Compile Remove="Application\**" />
    <Content Remove="Application\**" />
    <EmbeddedResource Remove="Application\**" />
    <None Remove="Application\**" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.11" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="10.2.3" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
    <ProjectReference Include="..\Booking.Application\Booking.Application.csproj" />
    <ProjectReference Include="..\Booking.Contract\Booking.Contract.csproj" />
    <ProjectReference Include="..\Booking.Domain\Booking.Domain.csproj" />
    <ProjectReference Include="..\Booking.Infrastructure\Booking.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Folder Include="Controllers\" />
  </ItemGroup>

</Project>
```

- [ ] **Step 6: Register the 5 projects in `BaseRepo.slnx`**

Add a new `<Folder Name="/Booking/">` block (alongside the existing `/AdministrativeUnit/`, `/Gateway/`, `/Site/` folders — keep alphabetical position, right after `/AdministrativeUnit/`):

```xml
  <Folder Name="/Booking/">
    <Project Path="Booking/Booking.Application/Booking.Application.csproj" />
    <Project Path="Booking/Booking.Contract/Booking.Contract.csproj" />
    <Project Path="Booking/Booking.Domain/Booking.Domain.csproj" />
    <Project Path="Booking/Booking.Infrastructure/Booking.Infrastructure.csproj" />
    <Project Path="Booking/Booking.Api/Booking.Api.csproj" />
  </Folder>
```

- [ ] **Step 6.5: Create a placeholder `Booking/Booking.Api/Program.cs`** — `Booking.Api.csproj` uses `Microsoft.NET.Sdk.Web`, which implicitly requires an entry point (`OutputType=Exe`); with zero source files the build fails with CS5001 ("Program does not contain a static 'Main' method"), contradicting this task's own build-verification step below. This minimal placeholder exists only to satisfy that requirement — Task 2 Step 6 fully overwrites it with the real bootstrap content:

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();
```

- [ ] **Step 7: Verify the solution restores/builds with the new projects**

Run: `dotnet build BaseRepo.slnx`
Expected: build succeeds, 0 Error (new projects have no source files yet, so nothing to compile beyond the project shells).

- [ ] **Step 8: Commit**

```bash
git add Booking BaseRepo.slnx
git commit -m "chore: scaffold Booking microservice project structure"
```

---

### Task 2: Booking.Api bootstrap (Program.cs, config, DbContext, empty DI)

**Files:**
- Create: `Booking/Booking.Api/Program.cs`
- Create: `Booking/Booking.Api/appsettings.json`
- Create: `Booking/Booking.Api/appsettings.Serilog.json`
- Create: `Booking/Booking.Api/Properties/launchSettings.json`
- Create: `Booking/Booking.Api/Utils/AddRequiredHeaderParameter.cs`
- Create: `Booking/Booking.Infrastructure/DBContext/BookingDbContext.cs`
- Create: `Booking/Booking.Infrastructure/DependencyInjection.cs`

**Interfaces:**
- Consumes: `CommonLibs.Services.AddCommonServices<TDbContext>(IConfiguration, string applicationNamespace)`, `CommonLibs.Middlewares.{AuthorizationMiddleware,GlobalExceptionMiddleware}`, `CommonLibs.Logging.{AddDynamicLogging,UseDynamicLogging}`.
- Produces: `Booking.Infrastructure.DependencyInjection.AddBookingInfrastructure(IServiceCollection, IConfiguration)` — later tasks add `services.AddScoped<I{Entity}Repository, {Entity}Repository>()` lines inside it. `Booking.Infrastructure.DBContext.BookingDbContext` — later repositories take this as a constructor dependency.

- [ ] **Step 1: Create `BookingDbContext.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using CommonLibs.BaseDbContext;
using Microsoft.Extensions.Logging;

namespace Booking.Infrastructure.DBContext
{
    public class BookingDbContext : BaseContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options, ILogger<BookingDbContext> logger) : base(options, logger)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
```

(No `using Booking.Domain.Entities;` here — that namespace has zero declared types until Task 4 creates the first entity, and an empty/non-existent namespace in a `using` directive is a compile error (`CS0234`), not a harmless unused-import warning. Task 4 Step 6 imports it where it's actually needed.)

- [ ] **Step 2: Create `DependencyInjection.cs`** — deliberately **without** `using Booking.Application.Interfaces;`/`using Booking.Infrastructure.Repositories;` yet, for the same reason: neither namespace has any declared type until Task 4 creates `IFieldTypeRepository`/`FieldTypeRepository`. Task 4 Step 7 adds both `using` lines back in alongside the first registration.

```csharp
using CommonLibs.Options;
using CommonLibs.Services;
using CommonLibs.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Booking.Infrastructure.DBContext;
using System.IdentityModel.Tokens.Jwt;

namespace Booking.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEnsureServiceRunning();
            services.AddCommonServices<BookingDbContext>(configuration, "Booking.Application");
            services.AddScoped<JwtSecurityTokenHandler>();

            // Repository registrations added by later tasks (FieldType, Field, PricingRule).

            return services;
        }
    }
}
```

- [ ] **Step 3: Create `Booking/Booking.Api/Utils/AddRequiredHeaderParameter.cs`** (verbatim copy of the AdministrativeUnit.Api one, namespace adjusted)

```csharp
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;
namespace Booking.Api.Utils
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<IOpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Device-ID",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Device identifier",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Default = JsonValue.Create("DEV-DEVICE-2604")
                }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Device-Name",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Device name",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Default = JsonValue.Create(Environment.MachineName)
                }
            });
        }
    }
}
```

- [ ] **Step 4: Create `appsettings.Serilog.json`** (copy of AdministrativeUnit's, `Application`/log path renamed)

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] [{Application}] {Message:lj}{NewLine}{Exception}",
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/booking_log-.json",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "formatter": {
            "type": "Serilog.Templates.ExpressionTemplate, Serilog.Expressions",
            "template": "{ {@t: UtcDateTime(@t), LocalTime: @p['LocalTime'], @l: @l, Application: @p['Application'], CorrelationId: @p['CorrelationId'], Message: @m, Exception: @x, Properties: Rest()} }\n"
          }
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName" ],
    "Properties": {
      "Application": "Booking-Service"
    }
  }
}
```

- [ ] **Step 5: Create `appsettings.json`** — port **8003** (next free port after Site=8001, AdministrativeUnit=8002), own database `Booking`, **reusing the same encrypted SQL login ciphertext already committed for AdministrativeUnit** (same SQL Server instance/login, only `Database` differs — mirrors how AdministrativeUnit itself reused Site's login):

```json
{
  "AllowedHosts": "*",
  "Application": {
    "ApplicationId": "Booking",
    "ApplicationName": "Base Repo Booking",
    "Version": "1.0.0",
    "AccessKey": "BaseRepo__JWT__AccessKey",
    "RefreshKey": "BaseRepo__JWT__RefreshKey",
    "Audience": "BaseRepo__JWT__Audience",
    "Issuer": "BaseRepo__JWT__Issuer",
    "PrivateKey": "BaseRepo__PrivateKey",
    "PublicKey": "BaseRepo__PublicKey",
    "RedisConnectionString": "127.0.0.1:6379,abortConnect=false,connectTimeout=5000"
  },
  "Consul": {
    "Host": "http://localhost:80001",
    "ServiceName": "booking-service",
    "ServiceId": "booking-service-01",
    "ServiceIP": "localhost",
    "ServicePort": 8003,
    "HealthCheckUrl": "http://localhost:80/health"
  },
  "ConnectionInfo": {
    "Host": "localhost,1433",
    "Database": "Booking",
    "UserName": "c3PEXu/ljD9nFK4G+63Tuw==",
    "Password": "CeXUz2k6fi2ZvXEvR1yaeA=="
  },
  "WindowsServiceDependencies": [
    "_Redis_8012",
    "RabbitMQ"
  ],
  "Logging": {
    "Provider": "Serilog",
    "LogRequestBody": true,
    "LogResponseBody": true,
    "LogSuccessResponse": true,
    "LogFailedResponse": true,
    "MaxBodyLength": 5000
  }
}
```

- [ ] **Step 6: Overwrite `Program.cs`** — Task 1 left a minimal placeholder there (just enough to make `Booking.Api.csproj` buildable with zero other source files); replace its entire content with the real bootstrap below (mirrors `AdministrativeUnit.Api/Program.cs` exactly, names swapped, port 8003)

```csharp
using Booking.Infrastructure;
using Booking.Api.Utils;
using CommonLibs.Logging;
using CommonLibs.Middlewares;
using CommonLibs.Options;
using Microsoft.OpenApi;
using System.Security.Claims;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Serilog.json", optional: false, reloadOnChange: true);

builder.AddDynamicLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Booking API", Version = "v1" });
    options.CustomSchemaIds(type => type.ToString());

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter Access Token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
    options.OperationFilter<AddRequiredHeaderParameter>();
});

builder.Services.AddBookingInfrastructure(builder.Configuration);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8003);
});

builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOrAdminPolicy", policy => policy.RequireAssertion(context =>
        context.User.Identity?.IsAuthenticated == true
        && context.User.Claims.Any(c => c.Type == ClaimTypes.Role)));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseDynamicLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}

app.UseRouting();

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseMiddleware<AuthorizationMiddleware>();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
```

- [ ] **Step 6.5: Create `Properties/launchSettings.json`** — every sibling service (Gateway/Site/AdministrativeUnit) ships one; without it a bare `dotnet run` defaults to the `Production` environment, and `Program.cs`'s `if (app.Environment.IsDevelopment())` gate then hides `/swagger/v1/swagger.json` — which is exactly what Step 7 below checks. Mirrors `AdministrativeUnit.Api/Properties/launchSettings.json`, port/name swapped:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:53287",
      "sslPort": 44348
    }
  },
  "profiles": {
    "Booking": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:8003",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

- [ ] **Step 7: Verify it boots**

Run: `dotnet build BaseRepo.slnx` — expect 0 Error.
Run: `dotnet run --project Booking/Booking.Api/Booking.Api.csproj` (from repo root — no `ASPNETCORE_ENVIRONMENT` override needed now that Step 6.5's `launchSettings.json` sets it), then in another terminal: `curl http://localhost:8003/swagger/v1/swagger.json`
Expected: HTTP 200 with a Swagger JSON document (title "Booking API", no paths yet since no controllers exist). Stop the running process afterward (Ctrl+C).

- [ ] **Step 8: Commit**

```bash
git add Booking/Booking.Api Booking/Booking.Infrastructure/DBContext Booking/Booking.Infrastructure/DependencyInjection.cs
git commit -m "feat: bootstrap Booking.Api host (Program.cs, config, DbContext, empty DI)"
```

---

### Task 3: SQL schema for FieldType, Field, PricingRule

**Files:**
- Create: `CommonLibs/BaseSQL/booking/BaseBooking.sql`

**Interfaces:**
- Produces: tables `FieldType`, `Field`, `PricingRule` in a new `Booking` database — later Infrastructure repositories query these by name (no ORM migrations, matches the rest of the repo).

- [ ] **Step 1: Write `BaseBooking.sql`**

```sql
USE Booking
GO

-- =======================================================================
-- 1. FIELDTYPE — danh mục loại sân (bóng đá, cầu lông, pickleball...), dùng chung
-- =======================================================================
CREATE TABLE [dbo].[FieldType] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [FieldTypeId] VARCHAR(20) NOT NULL,
    [FieldTypeName] NVARCHAR(100) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_FieldType_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_FieldType] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_FieldType_FieldTypeId] UNIQUE ([FieldTypeId])
);
GO

CREATE INDEX [IX_FieldType_Status_CreatedDate_UpdatedDate]
ON [dbo].[FieldType]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 2. FIELD — sân cụ thể, thuộc 1 Site (Site service, tham chiếu qua Id, không FK vật lý
-- vì khác database) và 1 FieldType.
-- =======================================================================
CREATE TABLE [dbo].[Field] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [FieldId] VARCHAR(20) NOT NULL,
    [SiteId] VARCHAR(20) NOT NULL,
    [FieldTypeId] VARCHAR(20) NOT NULL,
    [FieldName] NVARCHAR(100) NOT NULL,
    -- OperationalStatus: 1=Active, 2=Maintenance. Tách biệt khỏi [Status] (soft-delete chung) — xem
    -- Booking.Domain.Entities.Field.FieldOperationalStatus.
    [OperationalStatus] INT NOT NULL CONSTRAINT [DF_Field_OperationalStatus] DEFAULT 1,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_Field_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_Field] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_Field_FieldId] UNIQUE ([FieldId]),
    CONSTRAINT [FK_Field_FieldType] FOREIGN KEY ([FieldTypeId]) REFERENCES [dbo].[FieldType]([FieldTypeId])
);
GO

CREATE NONCLUSTERED INDEX [IX_Field_SiteId] ON [dbo].[Field] ([SiteId]);
CREATE NONCLUSTERED INDEX [IX_Field_FieldTypeId] ON [dbo].[Field] ([FieldTypeId]);
CREATE INDEX [IX_Field_Status_CreatedDate_UpdatedDate]
ON [dbo].[Field]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 3. PRICINGRULE — giá theo khung giờ + ngày trong tuần cho 1 Field. Không có business
-- key riêng (định danh bằng AutoId thẳng) vì bản thân là 1 dòng cấu hình giá, không phải
-- thực thể nghiệp vụ độc lập được tham chiếu từ nơi khác.
-- =======================================================================
CREATE TABLE [dbo].[PricingRule] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [FieldId] VARCHAR(20) NOT NULL,
    -- 0=Sunday .. 6=Saturday (khớp System.DayOfWeek của .NET)
    [DayOfWeek] INT NOT NULL,
    [StartTime] TIME NOT NULL,
    [EndTime] TIME NOT NULL,
    [PricePerHour] DECIMAL(12,2) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_PricingRule_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_PricingRule] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [FK_PricingRule_Field] FOREIGN KEY ([FieldId]) REFERENCES [dbo].[Field]([FieldId])
);
GO

CREATE NONCLUSTERED INDEX [IX_PricingRule_FieldId] ON [dbo].[PricingRule] ([FieldId]);
CREATE INDEX [IX_PricingRule_Status_CreatedDate_UpdatedDate]
ON [dbo].[PricingRule]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO
```

- [ ] **Step 2: Run the script against the local SQL Server**

This is a manual step (matches how `BaseAdministrativeUnit.sql` was run — no auto-migration tooling in this repo):
1. Create an empty database named `Booking` on the same SQL Server instance used by Site/AdministrativeUnit (`localhost,1433`) if it doesn't exist yet.
2. Execute `CommonLibs/BaseSQL/booking/BaseBooking.sql` against it (SSMS, Azure Data Studio, or `sqlcmd`).
3. Confirm 3 tables exist: `SELECT name FROM sys.tables;` should list `FieldType`, `Field`, `PricingRule`.

- [ ] **Step 3: Commit**

```bash
git add CommonLibs/BaseSQL/booking/BaseBooking.sql
git commit -m "feat: add Booking database schema (FieldType, Field, PricingRule)"
```

---

### Task 4: FieldType — full vertical slice (Domain → Contract → Application → Infrastructure → Presentation)

**Files:**
- Create: `Booking/Booking.Domain/Entities/FieldType.cs`
- Create: `Booking/Booking.Contract/DTOs/FieldTypeDTO.cs`
- Create: `Booking/Booking.Application/Interfaces/IFieldTypeRepository.cs`
- Create: `Booking/Booking.Application/Features/FieldType/CreateFieldType/CreateFieldTypeCommand.cs`
- Create: `Booking/Booking.Application/Features/FieldType/CreateFieldType/CreateFieldTypeCommandHandler.cs`
- Create: `Booking/Booking.Application/Features/FieldType/UpdateFieldType/UpdateFieldTypeCommand.cs`
- Create: `Booking/Booking.Application/Features/FieldType/UpdateFieldType/UpdateFieldTypeCommandHandler.cs`
- Create: `Booking/Booking.Application/Features/FieldType/GetFieldTypeById/GetFieldTypeByIdQuery.cs`
- Create: `Booking/Booking.Application/Features/FieldType/GetFieldTypeById/GetFieldTypeByIdQueryHandler.cs`
- Create: `Booking/Booking.Application/Features/FieldType/GetAllFieldTypes/GetAllFieldTypesQuery.cs`
- Create: `Booking/Booking.Application/Features/FieldType/GetAllFieldTypes/GetAllFieldTypesQueryHandler.cs`
- Create: `Booking/Booking.Application/Features/FieldType/DeleteFieldType/DeleteFieldTypeCommand.cs`
- Create: `Booking/Booking.Application/Features/FieldType/DeleteFieldType/DeleteFieldTypeCommandHandler.cs`
- Create: `Booking/Booking.Application/Mappings/BookingMapping.cs`
- Create: `Booking/Booking.Infrastructure/Repositories/FieldTypeRepository.cs`
- Modify: `Booking/Booking.Infrastructure/DependencyInjection.cs`
- Create: `Booking/Booking.Api/Controllers/FieldTypeController.cs`

**Interfaces:**
- Consumes: `BaseRepository<T>` ctor shape, `_dbContext.{ExecuteAsync,QueryFirstOrDefaultAsync,QueryAsync,ExecuteScalarAsync}` (Dapper extensions on `BaseContext`), `input.SetAuditInfo(...)`.
- Produces: `IFieldTypeRepository` methods `CreateFieldType`, `UpdateFieldType`, `GetFieldTypeById(string fieldTypeId, CancellationToken)`, `GetAllFieldTypes(FieldTypeFilterDTO, CancellationToken)`, `DeleteFieldType(DeleteFieldTypeInputDTO, CancellationToken)` — `Field` (Task 5) reads `FieldTypeOutputDTO` shape when joining.

- [ ] **Step 1: Domain entity**

```csharp
using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace Booking.Domain.Entities
{
    public class FieldType : BaseEntity
    {
        public string FieldTypeId { get; private set; } = null!;
        public string FieldTypeName { get; private set; } = null!;

        protected FieldType()
        {
        }

        public FieldType(string fieldTypeId, string fieldTypeName)
        {
            SetFieldTypeId(fieldTypeId);
            SetFieldTypeName(fieldTypeName);
        }

        public FieldType(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetFieldTypeId(row.Field<string>("FieldTypeId") ?? throw new AppException("FieldTypeId is required"));
            SetFieldTypeName(row.Field<string>("FieldTypeName") ?? throw new AppException("FieldTypeName is required"));
        }

        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetFieldTypeId(string fieldTypeId)
        {
            if (string.IsNullOrWhiteSpace(fieldTypeId))
                throw new AppException("FieldTypeId could not be empty");

            FieldTypeId = fieldTypeId.Trim();
        }

        public void SetFieldTypeName(string fieldTypeName)
        {
            if (string.IsNullOrWhiteSpace(fieldTypeName))
                throw new AppException("FieldTypeName could not be empty");

            FieldTypeName = fieldTypeName.Trim();
        }
    }
}
```

- [ ] **Step 2: Contract DTOs**

```csharp
using CommonLibs.DTOs;

namespace Booking.Contract.DTOs
{
    #region Input DTOs
    public class FieldTypeInputDTO : BaseInputDTO
    {
        public string FieldTypeId { get; set; } = null!;
        public string FieldTypeName { get; set; } = null!;

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["FieldTypeId"] = nameof(FieldTypeId),
            ["FieldTypeName"] = nameof(FieldTypeName),
            ["Status"] = nameof(Status)
        };
    }

    public class DeleteFieldTypeInputDTO : BaseInputDTO
    {
        public string? FieldTypeId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class FieldTypeOutputDTO : BaseOutputDTO
    {
        public string FieldTypeId { get; set; } = null!;
        public string FieldTypeName { get; set; } = null!;
    }
    #endregion

    #region Filter DTOs
    public class FieldTypeFilterDTO : BaseFilterDTO
    {
        public string? FieldTypeName { get; set; }
    }
    #endregion
}
```

- [ ] **Step 3: Repository interface**

```csharp
using CommonLibs.DTOs;
using Booking.Contract.DTOs;

namespace Booking.Application.Interfaces
{
    public interface IFieldTypeRepository
    {
        Task<FieldTypeOutputDTO> CreateFieldType(FieldTypeInputDTO input, CancellationToken cancellationToken);
        Task<FieldTypeOutputDTO> UpdateFieldType(FieldTypeInputDTO input, CancellationToken cancellationToken);
        Task<FieldTypeOutputDTO> GetFieldTypeById(string fieldTypeId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<FieldTypeOutputDTO>> GetAllFieldTypes(FieldTypeFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteFieldType(DeleteFieldTypeInputDTO input, CancellationToken cancellationToken);
    }
}
```

- [ ] **Step 4: CQRS — Create/Update/GetById/GetAll/Delete (Command/Query + thin Handler each)**

```csharp
// Booking/Booking.Application/Features/FieldType/CreateFieldType/CreateFieldTypeCommand.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.CreateFieldType
{
    public class CreateFieldTypeCommand : IRequest<FieldTypeOutputDTO>
    {
        public FieldTypeInputDTO Data { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/FieldType/CreateFieldType/CreateFieldTypeCommandHandler.cs
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.CreateFieldType
{
    public class CreateFieldTypeCommandHandler : IRequestHandler<CreateFieldTypeCommand, FieldTypeOutputDTO>
    {
        private readonly IFieldTypeRepository _repository;

        public CreateFieldTypeCommandHandler(IFieldTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldTypeOutputDTO> Handle(CreateFieldTypeCommand request, CancellationToken cancellationToken)
        {
            return await _repository.CreateFieldType(request.Data, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/FieldType/UpdateFieldType/UpdateFieldTypeCommand.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.UpdateFieldType
{
    public class UpdateFieldTypeCommand : IRequest<FieldTypeOutputDTO>
    {
        public FieldTypeInputDTO Data { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/FieldType/UpdateFieldType/UpdateFieldTypeCommandHandler.cs
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.UpdateFieldType
{
    public class UpdateFieldTypeCommandHandler : IRequestHandler<UpdateFieldTypeCommand, FieldTypeOutputDTO>
    {
        private readonly IFieldTypeRepository _repository;

        public UpdateFieldTypeCommandHandler(IFieldTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldTypeOutputDTO> Handle(UpdateFieldTypeCommand request, CancellationToken cancellationToken)
        {
            return await _repository.UpdateFieldType(request.Data, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/FieldType/GetFieldTypeById/GetFieldTypeByIdQuery.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.GetFieldTypeById
{
    public class GetFieldTypeByIdQuery : IRequest<FieldTypeOutputDTO>
    {
        public string Data { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/FieldType/GetFieldTypeById/GetFieldTypeByIdQueryHandler.cs
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.GetFieldTypeById
{
    public class GetFieldTypeByIdQueryHandler : IRequestHandler<GetFieldTypeByIdQuery, FieldTypeOutputDTO>
    {
        private readonly IFieldTypeRepository _repository;

        public GetFieldTypeByIdQueryHandler(IFieldTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldTypeOutputDTO> Handle(GetFieldTypeByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetFieldTypeById(request.Data, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/FieldType/GetAllFieldTypes/GetAllFieldTypesQuery.cs
using CommonLibs.DTOs;
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.GetAllFieldTypes
{
    public class GetAllFieldTypesQuery : IRequest<BaseFilterOutput<FieldTypeOutputDTO>>
    {
        public FieldTypeFilterDTO Filter { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/FieldType/GetAllFieldTypes/GetAllFieldTypesQueryHandler.cs
using CommonLibs.DTOs;
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.GetAllFieldTypes
{
    public class GetAllFieldTypesQueryHandler : IRequestHandler<GetAllFieldTypesQuery, BaseFilterOutput<FieldTypeOutputDTO>>
    {
        private readonly IFieldTypeRepository _repository;

        public GetAllFieldTypesQueryHandler(IFieldTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<FieldTypeOutputDTO>> Handle(GetAllFieldTypesQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllFieldTypes(request.Filter, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/FieldType/DeleteFieldType/DeleteFieldTypeCommand.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.FieldType.DeleteFieldType
{
    public class DeleteFieldTypeCommand : IRequest<string>
    {
        public DeleteFieldTypeInputDTO Data { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/FieldType/DeleteFieldType/DeleteFieldTypeCommandHandler.cs
using MediatR;
using Booking.Application.Interfaces;

namespace Booking.Application.Features.FieldType.DeleteFieldType
{
    public class DeleteFieldTypeCommandHandler : IRequestHandler<DeleteFieldTypeCommand, string>
    {
        private readonly IFieldTypeRepository _repository;

        public DeleteFieldTypeCommandHandler(IFieldTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteFieldTypeCommand request, CancellationToken cancellationToken)
        {
            return await _repository.DeleteFieldType(request.Data, cancellationToken);
        }
    }
}
```

- [ ] **Step 5: AutoMapper profile**

```csharp
// Booking/Booking.Application/Mappings/BookingMapping.cs
using AutoMapper;
using Booking.Contract.DTOs;
using Booking.Domain.Entities;

namespace Booking.Application.Mappings
{
    public class BookingMapping : Profile
    {
        public BookingMapping()
        {
            CreateMap<FieldType, FieldTypeOutputDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<FieldTypeInputDTO, FieldTypeOutputDTO>();
        }
    }
}
```

- [ ] **Step 6: Infrastructure repository**

```csharp
using AutoMapper;
using CommonLibs.DTOs;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Services;
using CommonLibs.Utils;
using Dapper;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;
using Booking.Domain.Entities;
using Booking.Infrastructure.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;
using System.Text;

namespace Booking.Infrastructure.Repositories
{
    public class FieldTypeRepository : BaseRepository<FieldTypeRepository>, IFieldTypeRepository
    {
        private readonly BookingDbContext _dbContext;
        private readonly string _errorPrefix = "FieldType_Repo_";

        public FieldTypeRepository(
            IMapper mapper,
            ILogger<FieldTypeRepository> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContext,
            BookingDbContext dbContext,
            IOptions<AppOptions> options,
            IAuditContext auditContext,
            IJwtTokenService jwtTokenService)
            : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Repository Methods

        public async Task<FieldTypeOutputDTO> CreateFieldType(FieldTypeInputDTO input, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}CreateFieldType-01] Input is null.", HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(input.FieldTypeId))
                throw new AppException($"[{_errorPrefix}CreateFieldType-02] FieldTypeId is required.", HttpStatusCode.BadRequest);

            if (await CheckFieldTypeExists(input.FieldTypeId, cancellationToken))
                throw new AppException($"[{_errorPrefix}CreateFieldType-03] FieldType with Id '{input.FieldTypeId}' already exists.", HttpStatusCode.Conflict);

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            int rowsAffected = await _dbContext.ExecuteAsync(@"
                INSERT INTO FieldType (FieldTypeId, FieldTypeName, CreatedDate, CreatedBy, DeviceName, DeviceId)
                VALUES (@FieldTypeId, @FieldTypeName, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
            ", new
            {
                input.FieldTypeId,
                input.FieldTypeName,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = input.CreatedBy,
                DeviceName = input.DeviceName,
                DeviceId = input.DeviceId,
            }, cancellationToken);

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}CreateFieldType-04] Failed to create FieldType.", HttpStatusCode.InternalServerError);

            return _mapper.Map<FieldTypeOutputDTO>(input);
        }

        public async Task<FieldTypeOutputDTO> UpdateFieldType(FieldTypeInputDTO input, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}UpdateFieldType-01] Input is null.", HttpStatusCode.BadRequest);

            if (!await CheckFieldTypeExists(input.FieldTypeId, cancellationToken))
                throw new AppException($"[{_errorPrefix}UpdateFieldType-02] FieldType with Id '{input.FieldTypeId}' not found.", HttpStatusCode.NotFound);

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            int rowsAffected = await _dbContext.ExecuteAsync(@"
                UPDATE FieldType
                SET FieldTypeName = @FieldTypeName, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy,
                    DeviceName = @DeviceName, DeviceId = @DeviceId
                WHERE FieldTypeId = @FieldTypeId
            ", new
            {
                input.FieldTypeId,
                input.FieldTypeName,
                input.UpdatedDate,
                input.UpdatedBy,
                input.DeviceName,
                input.DeviceId,
            }, cancellationToken);

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}UpdateFieldType-03] Failed to update FieldType.", HttpStatusCode.InternalServerError);

            return _mapper.Map<FieldTypeOutputDTO>(input);
        }

        public async Task<FieldTypeOutputDTO> GetFieldTypeById(string fieldTypeId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.QueryFirstOrDefaultAsync<FieldType>(
                @"SELECT * FROM FieldType WHERE FieldTypeId = @FieldTypeId",
                new { FieldTypeId = fieldTypeId },
                cancellationToken);

            if (entity == null)
                throw new AppException($"[{_errorPrefix}GetFieldTypeById-01] FieldType with Id '{fieldTypeId}' not found.", HttpStatusCode.NotFound);

            return _mapper.Map<FieldTypeOutputDTO>(entity);
        }

        public async Task<BaseFilterOutput<FieldTypeOutputDTO>> GetAllFieldTypes(FieldTypeFilterDTO filter, CancellationToken cancellationToken)
        {
            var whereBuilder = new StringBuilder(" WHERE 1=1 ");
            var parameters = new DynamicParameters();

            if (filter.Status.HasValue)
            {
                whereBuilder.Append(" AND [Status] = @Status ");
                parameters.Add("Status", filter.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.FieldTypeName))
            {
                whereBuilder.Append(" AND FieldTypeName LIKE @FieldTypeName ");
                parameters.Add("FieldTypeName", $"%{filter.FieldTypeName.Trim()}%", DbType.String);
            }

            if (filter.FromDate.HasValue)
            {
                whereBuilder.Append(" AND CreatedDate >= @FromDate ");
                parameters.Add("FromDate", filter.FromDate.Value);
            }
            if (filter.ToDate.HasValue)
            {
                whereBuilder.Append(" AND CreatedDate <= @ToDate ");
                parameters.Add("ToDate", filter.ToDate.Value);
            }

            string countSql = $"SELECT COUNT(1) FROM FieldType {whereBuilder}";
            var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

            var filterOutput = new BaseFilterOutput<FieldTypeOutputDTO>
            {
                TotalRecords = totalCount,
                TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Records = new List<FieldTypeOutputDTO>()
            };

            if (totalCount == 0)
                return filterOutput;

            string orderByClause = filter.SortBy switch
            {
                Sort.AutoIdDes => " ORDER BY AutoId DESC ",
                _ => " ORDER BY AutoId ASC "
            };

            string querySql = $"SELECT * FROM FieldType {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
            parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

            var entities = await _dbContext.QueryAsync<FieldType>(querySql, parameters, cancellationToken);
            filterOutput.Records = entities.Select(e => _mapper.Map<FieldTypeOutputDTO>(e)).ToList();

            return filterOutput;
        }

        public async Task<string> DeleteFieldType(DeleteFieldTypeInputDTO input, CancellationToken cancellationToken)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.FieldTypeId))
                throw new AppException($"[{_errorPrefix}DeleteFieldType-01] FieldTypeId is required.", HttpStatusCode.BadRequest);

            if (!await CheckFieldTypeExists(input.FieldTypeId, cancellationToken))
                throw new AppException($"[{_errorPrefix}DeleteFieldType-02] FieldType with Id '{input.FieldTypeId}' not found.", HttpStatusCode.NotFound);

            int rowsAffected;
            if (input.IsSoftDelete)
            {
                rowsAffected = await _dbContext.ExecuteAsync(@"
                    UPDATE FieldType SET Status = -1, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy
                    WHERE FieldTypeId = @FieldTypeId
                ", new { input.FieldTypeId, UpdatedDate = DateTime.UtcNow, UpdatedBy = _auditContext.UserName }, cancellationToken);
            }
            else
            {
                rowsAffected = await _dbContext.ExecuteAsync(
                    @"DELETE FROM FieldType WHERE FieldTypeId = @FieldTypeId",
                    new { input.FieldTypeId }, cancellationToken);
            }

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}DeleteFieldType-03] Failed to delete FieldType.", HttpStatusCode.InternalServerError);

            return input.FieldTypeId!;
        }

        #endregion

        #region Helpers

        private async Task<bool> CheckFieldTypeExists(string fieldTypeId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.QueryFirstOrDefaultAsync<FieldType>(
                @"SELECT * FROM FieldType WHERE FieldTypeId = @FieldTypeId",
                new { FieldTypeId = fieldTypeId },
                cancellationToken);

            return entity != null;
        }

        #endregion
    }
}
```

- [ ] **Step 7: Register in DI** — in `Booking/Booking.Infrastructure/DependencyInjection.cs`: add the two `using` lines Task 2 deliberately omitted (right below the existing `using Booking.Infrastructure.DBContext;`), then replace the comment line with the registration:

```csharp
using Booking.Application.Interfaces;
using Booking.Infrastructure.Repositories;
```

```csharp
            services.AddScoped<IFieldTypeRepository, FieldTypeRepository>();
            // Repository registrations added by later tasks (Field, PricingRule).
```

- [ ] **Step 8: Controller** — read (`GetById`/`GetAll`) is public, mutating actions require staff auth + `[RequireFunction]`:

```csharp
using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Booking.Application.Features.FieldType.CreateFieldType;
using Booking.Application.Features.FieldType.DeleteFieldType;
using Booking.Application.Features.FieldType.GetAllFieldTypes;
using Booking.Application.Features.FieldType.GetFieldTypeById;
using Booking.Application.Features.FieldType.UpdateFieldType;
using Booking.Contract.DTOs;

namespace Booking.Api.Controllers
{
    [Route("api/field-type")]
    [ApiController]
    public class FieldTypeController : BaseController<FieldTypeController>
    {
        public FieldTypeController(ILogger<FieldTypeController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("CreateFieldType")]
        public async Task<IActionResult> CreateFieldType([FromBody] FieldTypeInputDTO data)
        {
            var result = await _mediator.Send(new CreateFieldTypeCommand { Data = data });
            return Ok(new Response<FieldTypeOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{fieldTypeId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("UpdateFieldType")]
        public async Task<IActionResult> UpdateFieldType([FromRoute] string fieldTypeId, [FromBody] FieldTypeInputDTO data)
        {
            data.FieldTypeId = fieldTypeId;
            var result = await _mediator.Send(new UpdateFieldTypeCommand { Data = data });
            return Ok(new Response<FieldTypeOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{fieldTypeId}")]
        public async Task<IActionResult> GetFieldType([FromRoute] string fieldTypeId)
        {
            var result = await _mediator.Send(new GetFieldTypeByIdQuery { Data = fieldTypeId });
            return Ok(new Response<FieldTypeOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllFieldTypes([FromQuery] FieldTypeFilterDTO filter)
        {
            var result = await _mediator.Send(new GetAllFieldTypesQuery { Filter = filter });
            return Ok(new Response<BaseFilterOutput<FieldTypeOutputDTO>>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{fieldTypeId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("DeleteFieldType")]
        public async Task<IActionResult> DeleteFieldType([FromRoute] string fieldTypeId, [FromBody] DeleteFieldTypeInputDTO data)
        {
            data.FieldTypeId = fieldTypeId;
            var result = await _mediator.Send(new DeleteFieldTypeCommand { Data = data });
            return Ok(new Response<string>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
```

- [ ] **Step 9: Build and manually smoke-test** (no automated test project exists in this repo — verification is manual, per `.claude/rules/06-testing.md`)

Run: `dotnet build BaseRepo.slnx` — expect 0 Error.
Run: `dotnet run --project Booking/Booking.Api/Booking.Api.csproj`, then:

```bash
curl -X POST http://localhost:8003/api/field-type \
  -H "Content-Type: application/json" \
  -H "X-Device-ID: DEV-1" -H "X-Device-Name: DEV-1" \
  -d "{\"fieldTypeId\":\"BONGDA\",\"fieldTypeName\":\"Bong da mini\"}"
```

Expected: `401 Unauthorized` (no `Authorization` header — this route requires staff auth, confirming the policy is actually enforced). Then:

```bash
curl http://localhost:8003/api/field-type
```

Expected: `200 OK` with `Response<BaseFilterOutput<FieldTypeOutputDTO>>`, `Records: []` (empty table, no auth needed — confirms the read route is public). Stop the process afterward.

- [ ] **Step 10: Commit**

```bash
git add Booking/Booking.Domain/Entities/FieldType.cs Booking/Booking.Contract/DTOs/FieldTypeDTO.cs \
        Booking/Booking.Application/Interfaces/IFieldTypeRepository.cs \
        Booking/Booking.Application/Features/FieldType Booking/Booking.Application/Mappings \
        Booking/Booking.Infrastructure/Repositories/FieldTypeRepository.cs \
        Booking/Booking.Infrastructure/DependencyInjection.cs \
        Booking/Booking.Api/Controllers/FieldTypeController.cs
git commit -m "feat: add FieldType CRUD (Domain, Contract, Application, Infrastructure, Presentation)"
```

---

### Task 5: Field — full vertical slice (SiteId taken from staff claim, not client input)

**Files:**
- Create: `Booking/Booking.Domain/Entities/Field.cs`
- Create: `Booking/Booking.Contract/DTOs/FieldDTO.cs`
- Create: `Booking/Booking.Application/Interfaces/IFieldRepository.cs`
- Create: `Booking/Booking.Application/Features/Field/CreateField/{CreateFieldCommand,CreateFieldCommandHandler}.cs`
- Create: `Booking/Booking.Application/Features/Field/UpdateField/{UpdateFieldCommand,UpdateFieldCommandHandler}.cs`
- Create: `Booking/Booking.Application/Features/Field/GetFieldById/{GetFieldByIdQuery,GetFieldByIdQueryHandler}.cs`
- Create: `Booking/Booking.Application/Features/Field/GetAllFields/{GetAllFieldsQuery,GetAllFieldsQueryHandler}.cs`
- Create: `Booking/Booking.Application/Features/Field/DeleteField/{DeleteFieldCommand,DeleteFieldCommandHandler}.cs`
- Modify: `Booking/Booking.Application/Mappings/BookingMapping.cs`
- Create: `Booking/Booking.Infrastructure/Repositories/FieldRepository.cs`
- Modify: `Booking/Booking.Infrastructure/DependencyInjection.cs`
- Create: `Booking/Booking.Api/Controllers/FieldController.cs`

**Interfaces:**
- Consumes: `BaseController<T>.CurrentSiteId` (claim `site_id`, set by the shared `AuthorizationMiddleware` — see `CommonLibs/Controllers/BaseController.cs`), `IFieldTypeRepository` pattern from Task 4 (not called directly — `FieldTypeId` is just stored as a string FK, no cross-repository call needed for Phase 1).
- Produces: `IFieldRepository` — `Field` rows carry `SiteId`; Task 6 (`PricingRule`) reads a `Field`'s `SiteId` via `GetFieldById` to enforce the same Site-scoping when creating pricing rules.

- [ ] **Step 1: Domain entity** — `FieldOperationalStatus` is a business enum, deliberately separate from `BaseEntity.Status` (soft-delete)

```csharp
using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace Booking.Domain.Entities
{
    public enum FieldOperationalStatus
    {
        Active = 1,
        Maintenance = 2
    }

    public class Field : BaseEntity
    {
        public string FieldId { get; private set; } = null!;
        public string SiteId { get; private set; } = null!;
        public string FieldTypeId { get; private set; } = null!;
        public string FieldName { get; private set; } = null!;
        public FieldOperationalStatus OperationalStatus { get; private set; } = FieldOperationalStatus.Active;

        protected Field()
        {
        }

        public Field(string fieldId, string siteId, string fieldTypeId, string fieldName, FieldOperationalStatus operationalStatus = FieldOperationalStatus.Active)
        {
            SetFieldId(fieldId);
            SetSiteId(siteId);
            SetFieldTypeId(fieldTypeId);
            SetFieldName(fieldName);
            SetOperationalStatus(operationalStatus);
        }

        public Field(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetFieldId(row.Field<string>("FieldId") ?? throw new AppException("FieldId is required"));
            SetSiteId(row.Field<string>("SiteId") ?? throw new AppException("SiteId is required"));
            SetFieldTypeId(row.Field<string>("FieldTypeId") ?? throw new AppException("FieldTypeId is required"));
            SetFieldName(row.Field<string>("FieldName") ?? throw new AppException("FieldName is required"));
            SetOperationalStatus((FieldOperationalStatus)row.Field<int>("OperationalStatus"));
        }

        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetFieldId(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new AppException("FieldId could not be empty");

            FieldId = fieldId.Trim();
        }

        public void SetSiteId(string siteId)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                throw new AppException("SiteId could not be empty");

            SiteId = siteId.Trim();
        }

        public void SetFieldTypeId(string fieldTypeId)
        {
            if (string.IsNullOrWhiteSpace(fieldTypeId))
                throw new AppException("FieldTypeId could not be empty");

            FieldTypeId = fieldTypeId.Trim();
        }

        public void SetFieldName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new AppException("FieldName could not be empty");

            FieldName = fieldName.Trim();
        }

        public void SetOperationalStatus(FieldOperationalStatus operationalStatus)
        {
            OperationalStatus = operationalStatus;
        }
    }
}
```

- [ ] **Step 2: Contract DTOs** — `FieldInputDTO` carries `SiteId` (needed by the repository insert), but the controller (Step 8) always overwrites it with `CurrentSiteId` before dispatch, never trusting a client-supplied value

```csharp
using CommonLibs.DTOs;

namespace Booking.Contract.DTOs
{
    #region Input DTOs
    public class FieldInputDTO : BaseInputDTO
    {
        public string FieldId { get; set; } = null!;
        public string SiteId { get; set; } = null!;
        public string FieldTypeId { get; set; } = null!;
        public string FieldName { get; set; } = null!;
        public int OperationalStatus { get; set; } = 1;

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["FieldId"] = nameof(FieldId),
            ["FieldTypeId"] = nameof(FieldTypeId),
            ["FieldName"] = nameof(FieldName),
            ["Status"] = nameof(Status)
        };
    }

    public class DeleteFieldInputDTO : BaseInputDTO
    {
        public string? FieldId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class FieldOutputDTO : BaseOutputDTO
    {
        public string FieldId { get; set; } = null!;
        public string SiteId { get; set; } = null!;
        public string FieldTypeId { get; set; } = null!;
        public string FieldName { get; set; } = null!;
        public string OperationalStatus { get; set; } = null!;
    }
    #endregion

    #region Filter DTOs
    public class FieldFilterDTO : BaseFilterDTO
    {
        public string? FieldName { get; set; }
        public string? FieldTypeId { get; set; }
    }
    #endregion
}
```

- [ ] **Step 3: Repository interface**

```csharp
using CommonLibs.DTOs;
using Booking.Contract.DTOs;

namespace Booking.Application.Interfaces
{
    public interface IFieldRepository
    {
        Task<FieldOutputDTO> CreateField(FieldInputDTO input, CancellationToken cancellationToken);
        Task<FieldOutputDTO> UpdateField(FieldInputDTO input, string callerSiteId, CancellationToken cancellationToken);
        Task<FieldOutputDTO> GetFieldById(string fieldId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<FieldOutputDTO>> GetAllFields(FieldFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteField(DeleteFieldInputDTO input, string callerSiteId, CancellationToken cancellationToken);
    }
}
```

- [ ] **Step 4: CQRS** — `Command`s that mutate carry a separate `CallerSiteId` set by the controller from the claim (same shape as `ToggleSiteModuleCommand.SiteId` in the existing `Site` service):

```csharp
// Booking/Booking.Application/Features/Field/CreateField/CreateFieldCommand.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.CreateField
{
    public class CreateFieldCommand : IRequest<FieldOutputDTO>
    {
        public FieldInputDTO Data { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/Field/CreateField/CreateFieldCommandHandler.cs
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.CreateField
{
    public class CreateFieldCommandHandler : IRequestHandler<CreateFieldCommand, FieldOutputDTO>
    {
        private readonly IFieldRepository _repository;

        public CreateFieldCommandHandler(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldOutputDTO> Handle(CreateFieldCommand request, CancellationToken cancellationToken)
        {
            return await _repository.CreateField(request.Data, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/Field/UpdateField/UpdateFieldCommand.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.UpdateField
{
    public class UpdateFieldCommand : IRequest<FieldOutputDTO>
    {
        public FieldInputDTO Data { get; set; } = null!;
        public string CallerSiteId { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/Field/UpdateField/UpdateFieldCommandHandler.cs
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.UpdateField
{
    public class UpdateFieldCommandHandler : IRequestHandler<UpdateFieldCommand, FieldOutputDTO>
    {
        private readonly IFieldRepository _repository;

        public UpdateFieldCommandHandler(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldOutputDTO> Handle(UpdateFieldCommand request, CancellationToken cancellationToken)
        {
            return await _repository.UpdateField(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/Field/GetFieldById/GetFieldByIdQuery.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.GetFieldById
{
    public class GetFieldByIdQuery : IRequest<FieldOutputDTO>
    {
        public string Data { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/Field/GetFieldById/GetFieldByIdQueryHandler.cs
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.GetFieldById
{
    public class GetFieldByIdQueryHandler : IRequestHandler<GetFieldByIdQuery, FieldOutputDTO>
    {
        private readonly IFieldRepository _repository;

        public GetFieldByIdQueryHandler(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<FieldOutputDTO> Handle(GetFieldByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetFieldById(request.Data, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/Field/GetAllFields/GetAllFieldsQuery.cs
using CommonLibs.DTOs;
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.GetAllFields
{
    public class GetAllFieldsQuery : IRequest<BaseFilterOutput<FieldOutputDTO>>
    {
        public FieldFilterDTO Filter { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/Field/GetAllFields/GetAllFieldsQueryHandler.cs
using CommonLibs.DTOs;
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.GetAllFields
{
    public class GetAllFieldsQueryHandler : IRequestHandler<GetAllFieldsQuery, BaseFilterOutput<FieldOutputDTO>>
    {
        private readonly IFieldRepository _repository;

        public GetAllFieldsQueryHandler(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<FieldOutputDTO>> Handle(GetAllFieldsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllFields(request.Filter, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/Field/DeleteField/DeleteFieldCommand.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.Field.DeleteField
{
    public class DeleteFieldCommand : IRequest<string>
    {
        public DeleteFieldInputDTO Data { get; set; } = null!;
        public string CallerSiteId { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/Field/DeleteField/DeleteFieldCommandHandler.cs
using MediatR;
using Booking.Application.Interfaces;

namespace Booking.Application.Features.Field.DeleteField
{
    public class DeleteFieldCommandHandler : IRequestHandler<DeleteFieldCommand, string>
    {
        private readonly IFieldRepository _repository;

        public DeleteFieldCommandHandler(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeleteFieldCommand request, CancellationToken cancellationToken)
        {
            return await _repository.DeleteField(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
```

- [ ] **Step 5: Extend the AutoMapper profile** — add to the existing `BookingMapping` constructor from Task 4:

```csharp
            CreateMap<Domain.Entities.Field, FieldOutputDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.OperationalStatus, opt => opt.MapFrom(src => src.OperationalStatus.ToString()));
            CreateMap<FieldInputDTO, FieldOutputDTO>()
                .ForMember(dest => dest.OperationalStatus, opt => opt.MapFrom(src => ((Domain.Entities.FieldOperationalStatus)src.OperationalStatus).ToString()));
```

- [ ] **Step 6: Infrastructure repository** — `UpdateField`/`DeleteField` verify the target `Field.SiteId` matches `callerSiteId` before mutating (prevents a Site-A staff member touching Site-B's field once a second Site exists):

```csharp
using AutoMapper;
using CommonLibs.DTOs;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Services;
using CommonLibs.Utils;
using Dapper;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;
using Booking.Domain.Entities;
using Booking.Infrastructure.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;
using System.Text;

namespace Booking.Infrastructure.Repositories
{
    public class FieldRepository : BaseRepository<FieldRepository>, IFieldRepository
    {
        private readonly BookingDbContext _dbContext;
        private readonly string _errorPrefix = "Field_Repo_";

        public FieldRepository(
            IMapper mapper,
            ILogger<FieldRepository> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContext,
            BookingDbContext dbContext,
            IOptions<AppOptions> options,
            IAuditContext auditContext,
            IJwtTokenService jwtTokenService)
            : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Repository Methods

        public async Task<FieldOutputDTO> CreateField(FieldInputDTO input, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}CreateField-01] Input is null.", HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(input.FieldId))
                throw new AppException($"[{_errorPrefix}CreateField-02] FieldId is required.", HttpStatusCode.BadRequest);

            if (await CheckFieldExists(input.FieldId, cancellationToken))
                throw new AppException($"[{_errorPrefix}CreateField-03] Field with Id '{input.FieldId}' already exists.", HttpStatusCode.Conflict);

            var fieldType = await _dbContext.QueryFirstOrDefaultAsync<FieldType>(
                @"SELECT * FROM FieldType WHERE FieldTypeId = @FieldTypeId",
                new { input.FieldTypeId }, cancellationToken);
            if (fieldType == null)
                throw new AppException($"[{_errorPrefix}CreateField-04] FieldType with Id '{input.FieldTypeId}' not found.", HttpStatusCode.BadRequest);

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            int rowsAffected = await _dbContext.ExecuteAsync(@"
                INSERT INTO Field (FieldId, SiteId, FieldTypeId, FieldName, OperationalStatus, CreatedDate, CreatedBy, DeviceName, DeviceId)
                VALUES (@FieldId, @SiteId, @FieldTypeId, @FieldName, @OperationalStatus, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
            ", new
            {
                input.FieldId,
                input.SiteId,
                input.FieldTypeId,
                input.FieldName,
                input.OperationalStatus,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = input.CreatedBy,
                DeviceName = input.DeviceName,
                DeviceId = input.DeviceId,
            }, cancellationToken);

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}CreateField-05] Failed to create Field.", HttpStatusCode.InternalServerError);

            return _mapper.Map<FieldOutputDTO>(input);
        }

        public async Task<FieldOutputDTO> UpdateField(FieldInputDTO input, string callerSiteId, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}UpdateField-01] Input is null.", HttpStatusCode.BadRequest);

            var existing = await _dbContext.QueryFirstOrDefaultAsync<Field>(
                @"SELECT * FROM Field WHERE FieldId = @FieldId", new { input.FieldId }, cancellationToken);
            if (existing == null)
                throw new AppException($"[{_errorPrefix}UpdateField-02] Field with Id '{input.FieldId}' not found.", HttpStatusCode.NotFound);

            if (existing.SiteId != callerSiteId)
                throw new AppException($"[{_errorPrefix}UpdateField-03] Access denied. This Field does not belong to your Site.", HttpStatusCode.Forbidden);

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            int rowsAffected = await _dbContext.ExecuteAsync(@"
                UPDATE Field
                SET FieldTypeId = @FieldTypeId, FieldName = @FieldName, OperationalStatus = @OperationalStatus,
                    UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy, DeviceName = @DeviceName, DeviceId = @DeviceId
                WHERE FieldId = @FieldId
            ", new
            {
                input.FieldId,
                input.FieldTypeId,
                input.FieldName,
                input.OperationalStatus,
                input.UpdatedDate,
                input.UpdatedBy,
                input.DeviceName,
                input.DeviceId,
            }, cancellationToken);

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}UpdateField-04] Failed to update Field.", HttpStatusCode.InternalServerError);

            input.SiteId = existing.SiteId;
            return _mapper.Map<FieldOutputDTO>(input);
        }

        public async Task<FieldOutputDTO> GetFieldById(string fieldId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.QueryFirstOrDefaultAsync<Field>(
                @"SELECT * FROM Field WHERE FieldId = @FieldId", new { FieldId = fieldId }, cancellationToken);

            if (entity == null)
                throw new AppException($"[{_errorPrefix}GetFieldById-01] Field with Id '{fieldId}' not found.", HttpStatusCode.NotFound);

            return _mapper.Map<FieldOutputDTO>(entity);
        }

        public async Task<BaseFilterOutput<FieldOutputDTO>> GetAllFields(FieldFilterDTO filter, CancellationToken cancellationToken)
        {
            var whereBuilder = new StringBuilder(" WHERE 1=1 ");
            var parameters = new DynamicParameters();

            if (filter.Status.HasValue)
            {
                whereBuilder.Append(" AND [Status] = @Status ");
                parameters.Add("Status", filter.Status.Value);
            }
            if (!string.IsNullOrWhiteSpace(filter.FieldName))
            {
                whereBuilder.Append(" AND FieldName LIKE @FieldName ");
                parameters.Add("FieldName", $"%{filter.FieldName.Trim()}%", DbType.String);
            }
            if (!string.IsNullOrWhiteSpace(filter.FieldTypeId))
            {
                whereBuilder.Append(" AND FieldTypeId = @FieldTypeId ");
                parameters.Add("FieldTypeId", filter.FieldTypeId);
            }
            if (filter.FromDate.HasValue)
            {
                whereBuilder.Append(" AND CreatedDate >= @FromDate ");
                parameters.Add("FromDate", filter.FromDate.Value);
            }
            if (filter.ToDate.HasValue)
            {
                whereBuilder.Append(" AND CreatedDate <= @ToDate ");
                parameters.Add("ToDate", filter.ToDate.Value);
            }

            string countSql = $"SELECT COUNT(1) FROM Field {whereBuilder}";
            var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

            var filterOutput = new BaseFilterOutput<FieldOutputDTO>
            {
                TotalRecords = totalCount,
                TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Records = new List<FieldOutputDTO>()
            };

            if (totalCount == 0)
                return filterOutput;

            string orderByClause = filter.SortBy switch
            {
                Sort.AutoIdDes => " ORDER BY AutoId DESC ",
                _ => " ORDER BY AutoId ASC "
            };

            string querySql = $"SELECT * FROM Field {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
            parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

            var entities = await _dbContext.QueryAsync<Field>(querySql, parameters, cancellationToken);
            filterOutput.Records = entities.Select(e => _mapper.Map<FieldOutputDTO>(e)).ToList();

            return filterOutput;
        }

        public async Task<string> DeleteField(DeleteFieldInputDTO input, string callerSiteId, CancellationToken cancellationToken)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.FieldId))
                throw new AppException($"[{_errorPrefix}DeleteField-01] FieldId is required.", HttpStatusCode.BadRequest);

            var existing = await _dbContext.QueryFirstOrDefaultAsync<Field>(
                @"SELECT * FROM Field WHERE FieldId = @FieldId", new { input.FieldId }, cancellationToken);
            if (existing == null)
                throw new AppException($"[{_errorPrefix}DeleteField-02] Field with Id '{input.FieldId}' not found.", HttpStatusCode.NotFound);

            if (existing.SiteId != callerSiteId)
                throw new AppException($"[{_errorPrefix}DeleteField-03] Access denied. This Field does not belong to your Site.", HttpStatusCode.Forbidden);

            int rowsAffected;
            if (input.IsSoftDelete)
            {
                rowsAffected = await _dbContext.ExecuteAsync(@"
                    UPDATE Field SET Status = -1, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy
                    WHERE FieldId = @FieldId
                ", new { input.FieldId, UpdatedDate = DateTime.UtcNow, UpdatedBy = _auditContext.UserName }, cancellationToken);
            }
            else
            {
                rowsAffected = await _dbContext.ExecuteAsync(
                    @"DELETE FROM Field WHERE FieldId = @FieldId", new { input.FieldId }, cancellationToken);
            }

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}DeleteField-04] Failed to delete Field.", HttpStatusCode.InternalServerError);

            return input.FieldId!;
        }

        #endregion

        #region Helpers

        private async Task<bool> CheckFieldExists(string fieldId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.QueryFirstOrDefaultAsync<Field>(
                @"SELECT * FROM Field WHERE FieldId = @FieldId", new { FieldId = fieldId }, cancellationToken);

            return entity != null;
        }

        #endregion
    }
}
```

- [ ] **Step 7: Register in DI** — add below the `IFieldTypeRepository` line from Task 4:

```csharp
            services.AddScoped<IFieldRepository, FieldRepository>();
            // Repository registrations added by later tasks (PricingRule).
```

- [ ] **Step 8: Controller** — `CreateField`/`UpdateField`/`DeleteField` always source `SiteId` from `CurrentSiteId` (never the request body), throwing if the claim is missing:

```csharp
using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Booking.Application.Features.Field.CreateField;
using Booking.Application.Features.Field.DeleteField;
using Booking.Application.Features.Field.GetAllFields;
using Booking.Application.Features.Field.GetFieldById;
using Booking.Application.Features.Field.UpdateField;
using Booking.Contract.DTOs;

namespace Booking.Api.Controllers
{
    [Route("api/field")]
    [ApiController]
    public class FieldController : BaseController<FieldController>
    {
        public FieldController(ILogger<FieldController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("CreateField")]
        public async Task<IActionResult> CreateField([FromBody] FieldInputDTO data)
        {
            data.SiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new CreateFieldCommand { Data = data });
            return Ok(new Response<FieldOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{fieldId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("UpdateField")]
        public async Task<IActionResult> UpdateField([FromRoute] string fieldId, [FromBody] FieldInputDTO data)
        {
            data.FieldId = fieldId;
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new UpdateFieldCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<FieldOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{fieldId}")]
        public async Task<IActionResult> GetField([FromRoute] string fieldId)
        {
            var result = await _mediator.Send(new GetFieldByIdQuery { Data = fieldId });
            return Ok(new Response<FieldOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllFields([FromQuery] FieldFilterDTO filter)
        {
            var result = await _mediator.Send(new GetAllFieldsQuery { Filter = filter });
            return Ok(new Response<BaseFilterOutput<FieldOutputDTO>>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{fieldId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("DeleteField")]
        public async Task<IActionResult> DeleteField([FromRoute] string fieldId, [FromBody] DeleteFieldInputDTO data)
        {
            data.FieldId = fieldId;
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new DeleteFieldCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<string>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
```

- [ ] **Step 9: Build and manually smoke-test**

Run: `dotnet build BaseRepo.slnx` — expect 0 Error.
Run: `dotnet run --project Booking/Booking.Api/Booking.Api.csproj`, then:

```bash
curl http://localhost:8003/api/field
```

Expected: `200 OK`, `Response<BaseFilterOutput<FieldOutputDTO>>` with `Records: []` (public route, no auth needed). Confirm a write attempt without a token is rejected:

```bash
curl -X POST http://localhost:8003/api/field -H "Content-Type: application/json" -d "{}"
```

Expected: `401 Unauthorized`. Stop the process afterward.

- [ ] **Step 10: Commit**

```bash
git add Booking/Booking.Domain/Entities/Field.cs Booking/Booking.Contract/DTOs/FieldDTO.cs \
        Booking/Booking.Application/Interfaces/IFieldRepository.cs \
        Booking/Booking.Application/Features/Field Booking/Booking.Application/Mappings \
        Booking/Booking.Infrastructure/Repositories/FieldRepository.cs \
        Booking/Booking.Infrastructure/DependencyInjection.cs \
        Booking/Booking.Api/Controllers/FieldController.cs
git commit -m "feat: add Field CRUD, Site-scoped via claim (Domain, Contract, Application, Infrastructure, Presentation)"
```

---

### Task 6: PricingRule — full vertical slice (identified by AutoId, no separate business key)

**Files:**
- Create: `Booking/Booking.Domain/Entities/PricingRule.cs`
- Create: `Booking/Booking.Contract/DTOs/PricingRuleDTO.cs`
- Create: `Booking/Booking.Application/Interfaces/IPricingRuleRepository.cs`
- Create: `Booking/Booking.Application/Features/PricingRule/CreatePricingRule/{CreatePricingRuleCommand,CreatePricingRuleCommandHandler}.cs`
- Create: `Booking/Booking.Application/Features/PricingRule/UpdatePricingRule/{UpdatePricingRuleCommand,UpdatePricingRuleCommandHandler}.cs`
- Create: `Booking/Booking.Application/Features/PricingRule/GetPricingRuleById/{GetPricingRuleByIdQuery,GetPricingRuleByIdQueryHandler}.cs`
- Create: `Booking/Booking.Application/Features/PricingRule/GetAllPricingRules/{GetAllPricingRulesQuery,GetAllPricingRulesQueryHandler}.cs`
- Create: `Booking/Booking.Application/Features/PricingRule/DeletePricingRule/{DeletePricingRuleCommand,DeletePricingRuleCommandHandler}.cs`
- Modify: `Booking/Booking.Application/Mappings/BookingMapping.cs`
- Create: `Booking/Booking.Infrastructure/Repositories/PricingRuleRepository.cs`
- Modify: `Booking/Booking.Infrastructure/DependencyInjection.cs`
- Create: `Booking/Booking.Api/Controllers/PricingRuleController.cs`

**Interfaces:**
- Consumes: `Field` table (Task 5) — every mutation looks up the target `Field.SiteId` to enforce the same Site-scoping as Task 5, via a direct query (no cross-repository call — same DB, same pattern `FieldRepository.CreateField` used to validate `FieldTypeId`).
- Produces: `IPricingRuleRepository` — not consumed by other Phase-1-foundation tasks; the follow-up plan's `Booking` conflict-check logic will read this table directly when computing a slot's price.

- [ ] **Step 1: Domain entity** — no business-key string Id; `AutoId` (from `BaseEntity`) is the sole identifier

```csharp
using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace Booking.Domain.Entities
{
    public class PricingRule : BaseEntity
    {
        public string FieldId { get; private set; } = null!;
        public DayOfWeek DayOfWeek { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public decimal PricePerHour { get; private set; }

        protected PricingRule()
        {
        }

        public PricingRule(string fieldId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, decimal pricePerHour)
        {
            SetFieldId(fieldId);
            SetDayOfWeek(dayOfWeek);
            SetTimeRange(startTime, endTime);
            SetPricePerHour(pricePerHour);
        }

        public PricingRule(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetFieldId(row.Field<string>("FieldId") ?? throw new AppException("FieldId is required"));
            SetDayOfWeek((DayOfWeek)row.Field<int>("DayOfWeek"));
            SetTimeRange(row.Field<TimeSpan>("StartTime"), row.Field<TimeSpan>("EndTime"));
            SetPricePerHour(row.Field<decimal>("PricePerHour"));
        }

        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetFieldId(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new AppException("FieldId could not be empty");

            FieldId = fieldId.Trim();
        }

        public void SetDayOfWeek(DayOfWeek dayOfWeek)
        {
            DayOfWeek = dayOfWeek;
        }

        public void SetTimeRange(TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime <= startTime)
                throw new AppException("EndTime must be after StartTime");

            StartTime = startTime;
            EndTime = endTime;
        }

        public void SetPricePerHour(decimal pricePerHour)
        {
            if (pricePerHour <= 0)
                throw new AppException("PricePerHour must be greater than 0");

            PricePerHour = pricePerHour;
        }
    }
}
```

- [ ] **Step 2: Contract DTOs**

```csharp
using CommonLibs.DTOs;

namespace Booking.Contract.DTOs
{
    #region Input DTOs
    public class PricingRuleInputDTO : BaseInputDTO
    {
        public long AutoId { get; set; }
        public string FieldId { get; set; } = null!;
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal PricePerHour { get; set; }
    }

    public class DeletePricingRuleInputDTO : BaseInputDTO
    {
        public long AutoId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class PricingRuleOutputDTO : BaseOutputDTO
    {
        public string FieldId { get; set; } = null!;
        public string DayOfWeek { get; set; } = null!;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal PricePerHour { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class PricingRuleFilterDTO : BaseFilterDTO
    {
        public string? FieldId { get; set; }
        public int? DayOfWeek { get; set; }
    }
    #endregion
}
```

- [ ] **Step 3: Repository interface** — `AutoId`/`long` in place of the usual `string {Entity}Id`

```csharp
using CommonLibs.DTOs;
using Booking.Contract.DTOs;

namespace Booking.Application.Interfaces
{
    public interface IPricingRuleRepository
    {
        Task<PricingRuleOutputDTO> CreatePricingRule(PricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken);
        Task<PricingRuleOutputDTO> UpdatePricingRule(PricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken);
        Task<PricingRuleOutputDTO> GetPricingRuleById(long autoId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<PricingRuleOutputDTO>> GetAllPricingRules(PricingRuleFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeletePricingRule(DeletePricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken);
    }
}
```

- [ ] **Step 4: CQRS**

```csharp
// Booking/Booking.Application/Features/PricingRule/CreatePricingRule/CreatePricingRuleCommand.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.CreatePricingRule
{
    public class CreatePricingRuleCommand : IRequest<PricingRuleOutputDTO>
    {
        public PricingRuleInputDTO Data { get; set; } = null!;
        public string CallerSiteId { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/PricingRule/CreatePricingRule/CreatePricingRuleCommandHandler.cs
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.CreatePricingRule
{
    public class CreatePricingRuleCommandHandler : IRequestHandler<CreatePricingRuleCommand, PricingRuleOutputDTO>
    {
        private readonly IPricingRuleRepository _repository;

        public CreatePricingRuleCommandHandler(IPricingRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<PricingRuleOutputDTO> Handle(CreatePricingRuleCommand request, CancellationToken cancellationToken)
        {
            return await _repository.CreatePricingRule(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/PricingRule/UpdatePricingRule/UpdatePricingRuleCommand.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.UpdatePricingRule
{
    public class UpdatePricingRuleCommand : IRequest<PricingRuleOutputDTO>
    {
        public PricingRuleInputDTO Data { get; set; } = null!;
        public string CallerSiteId { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/PricingRule/UpdatePricingRule/UpdatePricingRuleCommandHandler.cs
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.UpdatePricingRule
{
    public class UpdatePricingRuleCommandHandler : IRequestHandler<UpdatePricingRuleCommand, PricingRuleOutputDTO>
    {
        private readonly IPricingRuleRepository _repository;

        public UpdatePricingRuleCommandHandler(IPricingRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<PricingRuleOutputDTO> Handle(UpdatePricingRuleCommand request, CancellationToken cancellationToken)
        {
            return await _repository.UpdatePricingRule(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/PricingRule/GetPricingRuleById/GetPricingRuleByIdQuery.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.GetPricingRuleById
{
    public class GetPricingRuleByIdQuery : IRequest<PricingRuleOutputDTO>
    {
        public long Data { get; set; }
    }
}
```

```csharp
// Booking/Booking.Application/Features/PricingRule/GetPricingRuleById/GetPricingRuleByIdQueryHandler.cs
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.GetPricingRuleById
{
    public class GetPricingRuleByIdQueryHandler : IRequestHandler<GetPricingRuleByIdQuery, PricingRuleOutputDTO>
    {
        private readonly IPricingRuleRepository _repository;

        public GetPricingRuleByIdQueryHandler(IPricingRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<PricingRuleOutputDTO> Handle(GetPricingRuleByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetPricingRuleById(request.Data, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/PricingRule/GetAllPricingRules/GetAllPricingRulesQuery.cs
using CommonLibs.DTOs;
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.GetAllPricingRules
{
    public class GetAllPricingRulesQuery : IRequest<BaseFilterOutput<PricingRuleOutputDTO>>
    {
        public PricingRuleFilterDTO Filter { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/PricingRule/GetAllPricingRules/GetAllPricingRulesQueryHandler.cs
using CommonLibs.DTOs;
using MediatR;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.GetAllPricingRules
{
    public class GetAllPricingRulesQueryHandler : IRequestHandler<GetAllPricingRulesQuery, BaseFilterOutput<PricingRuleOutputDTO>>
    {
        private readonly IPricingRuleRepository _repository;

        public GetAllPricingRulesQueryHandler(IPricingRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<PricingRuleOutputDTO>> Handle(GetAllPricingRulesQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllPricingRules(request.Filter, cancellationToken);
        }
    }
}
```

```csharp
// Booking/Booking.Application/Features/PricingRule/DeletePricingRule/DeletePricingRuleCommand.cs
using MediatR;
using Booking.Contract.DTOs;

namespace Booking.Application.Features.PricingRule.DeletePricingRule
{
    public class DeletePricingRuleCommand : IRequest<string>
    {
        public DeletePricingRuleInputDTO Data { get; set; } = null!;
        public string CallerSiteId { get; set; } = null!;
    }
}
```

```csharp
// Booking/Booking.Application/Features/PricingRule/DeletePricingRule/DeletePricingRuleCommandHandler.cs
using MediatR;
using Booking.Application.Interfaces;

namespace Booking.Application.Features.PricingRule.DeletePricingRule
{
    public class DeletePricingRuleCommandHandler : IRequestHandler<DeletePricingRuleCommand, string>
    {
        private readonly IPricingRuleRepository _repository;

        public DeletePricingRuleCommandHandler(IPricingRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DeletePricingRuleCommand request, CancellationToken cancellationToken)
        {
            return await _repository.DeletePricingRule(request.Data, request.CallerSiteId, cancellationToken);
        }
    }
}
```

- [ ] **Step 5: Extend `BookingMapping`** — add to the constructor:

```csharp
            CreateMap<Domain.Entities.PricingRule, PricingRuleOutputDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek.ToString()));
            CreateMap<PricingRuleInputDTO, PricingRuleOutputDTO>()
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => ((DayOfWeek)src.DayOfWeek).ToString()));
```

- [ ] **Step 6: Infrastructure repository**

```csharp
using AutoMapper;
using CommonLibs.DTOs;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Services;
using CommonLibs.Utils;
using Dapper;
using Booking.Application.Interfaces;
using Booking.Contract.DTOs;
using Booking.Domain.Entities;
using Booking.Infrastructure.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;
using System.Text;

namespace Booking.Infrastructure.Repositories
{
    public class PricingRuleRepository : BaseRepository<PricingRuleRepository>, IPricingRuleRepository
    {
        private readonly BookingDbContext _dbContext;
        private readonly string _errorPrefix = "PricingRule_Repo_";

        public PricingRuleRepository(
            IMapper mapper,
            ILogger<PricingRuleRepository> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContext,
            BookingDbContext dbContext,
            IOptions<AppOptions> options,
            IAuditContext auditContext,
            IJwtTokenService jwtTokenService)
            : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Repository Methods

        public async Task<PricingRuleOutputDTO> CreatePricingRule(PricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}CreatePricingRule-01] Input is null.", HttpStatusCode.BadRequest);

            var field = await GetFieldOwnedBySite(input.FieldId, callerSiteId, cancellationToken, "CreatePricingRule-02");

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            var newAutoId = await _dbContext.ExecuteScalarAsync<long>(@"
                INSERT INTO PricingRule (FieldId, DayOfWeek, StartTime, EndTime, PricePerHour, CreatedDate, CreatedBy, DeviceName, DeviceId)
                OUTPUT INSERTED.AutoId
                VALUES (@FieldId, @DayOfWeek, @StartTime, @EndTime, @PricePerHour, @CreatedDate, @CreatedBy, @DeviceName, @DeviceId)
            ", new
            {
                input.FieldId,
                input.DayOfWeek,
                input.StartTime,
                input.EndTime,
                input.PricePerHour,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = input.CreatedBy,
                DeviceName = input.DeviceName,
                DeviceId = input.DeviceId,
            }, cancellationToken);

            if (newAutoId <= 0)
                throw new AppException($"[{_errorPrefix}CreatePricingRule-03] Failed to create PricingRule.", HttpStatusCode.InternalServerError);

            input.AutoId = newAutoId;
            return _mapper.Map<PricingRuleOutputDTO>(input);
        }

        public async Task<PricingRuleOutputDTO> UpdatePricingRule(PricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken)
        {
            if (input == null)
                throw new AppException($"[{_errorPrefix}UpdatePricingRule-01] Input is null.", HttpStatusCode.BadRequest);

            var existing = await _dbContext.QueryFirstOrDefaultAsync<PricingRule>(
                @"SELECT * FROM PricingRule WHERE AutoId = @AutoId", new { input.AutoId }, cancellationToken);
            if (existing == null)
                throw new AppException($"[{_errorPrefix}UpdatePricingRule-02] PricingRule '{input.AutoId}' not found.", HttpStatusCode.NotFound);

            await GetFieldOwnedBySite(input.FieldId, callerSiteId, cancellationToken, "UpdatePricingRule-03");

            input.SetAuditInfo(_auditContext.UserName, _auditContext.DeviceId, _auditContext.DeviceName);

            int rowsAffected = await _dbContext.ExecuteAsync(@"
                UPDATE PricingRule
                SET FieldId = @FieldId, DayOfWeek = @DayOfWeek, StartTime = @StartTime, EndTime = @EndTime,
                    PricePerHour = @PricePerHour, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy,
                    DeviceName = @DeviceName, DeviceId = @DeviceId
                WHERE AutoId = @AutoId
            ", new
            {
                input.AutoId,
                input.FieldId,
                input.DayOfWeek,
                input.StartTime,
                input.EndTime,
                input.PricePerHour,
                input.UpdatedDate,
                input.UpdatedBy,
                input.DeviceName,
                input.DeviceId,
            }, cancellationToken);

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}UpdatePricingRule-04] Failed to update PricingRule.", HttpStatusCode.InternalServerError);

            return _mapper.Map<PricingRuleOutputDTO>(input);
        }

        public async Task<PricingRuleOutputDTO> GetPricingRuleById(long autoId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.QueryFirstOrDefaultAsync<PricingRule>(
                @"SELECT * FROM PricingRule WHERE AutoId = @AutoId", new { AutoId = autoId }, cancellationToken);

            if (entity == null)
                throw new AppException($"[{_errorPrefix}GetPricingRuleById-01] PricingRule '{autoId}' not found.", HttpStatusCode.NotFound);

            return _mapper.Map<PricingRuleOutputDTO>(entity);
        }

        public async Task<BaseFilterOutput<PricingRuleOutputDTO>> GetAllPricingRules(PricingRuleFilterDTO filter, CancellationToken cancellationToken)
        {
            var whereBuilder = new StringBuilder(" WHERE 1=1 ");
            var parameters = new DynamicParameters();

            if (filter.Status.HasValue)
            {
                whereBuilder.Append(" AND [Status] = @Status ");
                parameters.Add("Status", filter.Status.Value);
            }
            if (!string.IsNullOrWhiteSpace(filter.FieldId))
            {
                whereBuilder.Append(" AND FieldId = @FieldId ");
                parameters.Add("FieldId", filter.FieldId);
            }
            if (filter.DayOfWeek.HasValue)
            {
                whereBuilder.Append(" AND DayOfWeek = @DayOfWeek ");
                parameters.Add("DayOfWeek", filter.DayOfWeek.Value);
            }
            if (filter.FromDate.HasValue)
            {
                whereBuilder.Append(" AND CreatedDate >= @FromDate ");
                parameters.Add("FromDate", filter.FromDate.Value);
            }
            if (filter.ToDate.HasValue)
            {
                whereBuilder.Append(" AND CreatedDate <= @ToDate ");
                parameters.Add("ToDate", filter.ToDate.Value);
            }

            string countSql = $"SELECT COUNT(1) FROM PricingRule {whereBuilder}";
            var totalCount = await _dbContext.ExecuteScalarAsync<long>(countSql, parameters, cancellationToken);

            var filterOutput = new BaseFilterOutput<PricingRuleOutputDTO>
            {
                TotalRecords = totalCount,
                TotalPages = filter.PageSize > 0 ? (long)Math.Ceiling((double)totalCount / filter.PageSize) : 0,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Records = new List<PricingRuleOutputDTO>()
            };

            if (totalCount == 0)
                return filterOutput;

            string orderByClause = filter.SortBy switch
            {
                Sort.AutoIdDes => " ORDER BY AutoId DESC ",
                _ => " ORDER BY AutoId ASC "
            };

            string querySql = $"SELECT * FROM PricingRule {whereBuilder} {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            parameters.Add("Offset", Math.Max(0, filter.PageNumber - 1) * filter.PageSize);
            parameters.Add("PageSize", filter.PageSize > 0 ? filter.PageSize : 20);

            var entities = await _dbContext.QueryAsync<PricingRule>(querySql, parameters, cancellationToken);
            filterOutput.Records = entities.Select(e => _mapper.Map<PricingRuleOutputDTO>(e)).ToList();

            return filterOutput;
        }

        public async Task<string> DeletePricingRule(DeletePricingRuleInputDTO input, string callerSiteId, CancellationToken cancellationToken)
        {
            if (input == null || input.AutoId <= 0)
                throw new AppException($"[{_errorPrefix}DeletePricingRule-01] AutoId is required.", HttpStatusCode.BadRequest);

            var existing = await _dbContext.QueryFirstOrDefaultAsync<PricingRule>(
                @"SELECT * FROM PricingRule WHERE AutoId = @AutoId", new { input.AutoId }, cancellationToken);
            if (existing == null)
                throw new AppException($"[{_errorPrefix}DeletePricingRule-02] PricingRule '{input.AutoId}' not found.", HttpStatusCode.NotFound);

            await GetFieldOwnedBySite(existing.FieldId, callerSiteId, cancellationToken, "DeletePricingRule-03");

            int rowsAffected;
            if (input.IsSoftDelete)
            {
                rowsAffected = await _dbContext.ExecuteAsync(@"
                    UPDATE PricingRule SET Status = -1, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy
                    WHERE AutoId = @AutoId
                ", new { input.AutoId, UpdatedDate = DateTime.UtcNow, UpdatedBy = _auditContext.UserName }, cancellationToken);
            }
            else
            {
                rowsAffected = await _dbContext.ExecuteAsync(
                    @"DELETE FROM PricingRule WHERE AutoId = @AutoId", new { input.AutoId }, cancellationToken);
            }

            if (rowsAffected <= 0)
                throw new AppException($"[{_errorPrefix}DeletePricingRule-04] Failed to delete PricingRule.", HttpStatusCode.InternalServerError);

            return input.AutoId.ToString();
        }

        #endregion

        #region Helpers

        /// <summary>Loads the Field for <paramref name="fieldId"/> and throws unless it belongs to <paramref name="callerSiteId"/> — mirrors the check in FieldRepository.UpdateField/DeleteField, applied here so pricing can't be set on another Site's Field.</summary>
        private async Task<Field> GetFieldOwnedBySite(string fieldId, string callerSiteId, CancellationToken cancellationToken, string errorStep)
        {
            var field = await _dbContext.QueryFirstOrDefaultAsync<Field>(
                @"SELECT * FROM Field WHERE FieldId = @FieldId", new { FieldId = fieldId }, cancellationToken);

            if (field == null)
                throw new AppException($"[{_errorPrefix}{errorStep}] Field with Id '{fieldId}' not found.", HttpStatusCode.BadRequest);

            if (field.SiteId != callerSiteId)
                throw new AppException($"[{_errorPrefix}{errorStep}] Access denied. This Field does not belong to your Site.", HttpStatusCode.Forbidden);

            return field;
        }

        #endregion
    }
}
```

- [ ] **Step 7: Register in DI** — add below the `IFieldRepository` line from Task 5:

```csharp
            services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();
```

- [ ] **Step 8: Controller** — read is public, writes are staff-only and Site-scoped (same `CurrentSiteId` pattern as `FieldController`), identified by `autoId` (long) instead of a string Id:

```csharp
using CommonLibs.Comtrollers;
using CommonLibs.DTOs;
using CommonLibs.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Booking.Application.Features.PricingRule.CreatePricingRule;
using Booking.Application.Features.PricingRule.DeletePricingRule;
using Booking.Application.Features.PricingRule.GetAllPricingRules;
using Booking.Application.Features.PricingRule.GetPricingRuleById;
using Booking.Application.Features.PricingRule.UpdatePricingRule;
using Booking.Contract.DTOs;

namespace Booking.Api.Controllers
{
    [Route("api/pricing-rule")]
    [ApiController]
    public class PricingRuleController : BaseController<PricingRuleController>
    {
        public PricingRuleController(ILogger<PricingRuleController> logger, IMediator mediator, IHttpContextAccessor httpContext) : base(logger, mediator, httpContext)
        {
        }

        [HttpPost("")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("CreatePricingRule")]
        public async Task<IActionResult> CreatePricingRule([FromBody] PricingRuleInputDTO data)
        {
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new CreatePricingRuleCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<PricingRuleOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpPut("{autoId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("UpdatePricingRule")]
        public async Task<IActionResult> UpdatePricingRule([FromRoute] long autoId, [FromBody] PricingRuleInputDTO data)
        {
            data.AutoId = autoId;
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new UpdatePricingRuleCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<PricingRuleOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("{autoId}")]
        public async Task<IActionResult> GetPricingRule([FromRoute] long autoId)
        {
            var result = await _mediator.Send(new GetPricingRuleByIdQuery { Data = autoId });
            return Ok(new Response<PricingRuleOutputDTO>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllPricingRules([FromQuery] PricingRuleFilterDTO filter)
        {
            var result = await _mediator.Send(new GetAllPricingRulesQuery { Filter = filter });
            return Ok(new Response<BaseFilterOutput<PricingRuleOutputDTO>>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }

        [HttpDelete("{autoId}")]
        [Authorize(Policy = "UserOrAdminPolicy")]
        [RequireFunction("DeletePricingRule")]
        public async Task<IActionResult> DeletePricingRule([FromRoute] long autoId, [FromBody] DeletePricingRuleInputDTO data)
        {
            data.AutoId = autoId;
            var callerSiteId = CurrentSiteId ?? throw new AppException("Missing site_id claim.", System.Net.HttpStatusCode.Forbidden);
            var result = await _mediator.Send(new DeletePricingRuleCommand { Data = data, CallerSiteId = callerSiteId });
            return Ok(new Response<string>
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully",
                Data = result
            });
        }
    }
}
```

- [ ] **Step 9: Build and manually smoke-test**

Run: `dotnet build BaseRepo.slnx` — expect 0 Error.
Run: `dotnet run --project Booking/Booking.Api/Booking.Api.csproj`, then:

```bash
curl http://localhost:8003/api/pricing-rule
```

Expected: `200 OK`, `Records: []` (public route). Stop the process afterward.

- [ ] **Step 10: Commit**

```bash
git add Booking/Booking.Domain/Entities/PricingRule.cs Booking/Booking.Contract/DTOs/PricingRuleDTO.cs \
        Booking/Booking.Application/Interfaces/IPricingRuleRepository.cs \
        Booking/Booking.Application/Features/PricingRule Booking/Booking.Application/Mappings \
        Booking/Booking.Infrastructure/Repositories/PricingRuleRepository.cs \
        Booking/Booking.Infrastructure/DependencyInjection.cs \
        Booking/Booking.Api/Controllers/PricingRuleController.cs
git commit -m "feat: add PricingRule CRUD, Site-scoped via owning Field (Domain, Contract, Application, Infrastructure, Presentation)"
```

---

### Task 7: Gateway wiring — routes, cluster, swagger aggregation

**Files:**
- Modify: `Gateway/Gateway.Api/appsettings.json`
- Modify: `Gateway/Gateway.Api/Program.cs`

**Interfaces:**
- Consumes: `Booking.Api` running on port 8003 (Task 2), controllers/routes from Tasks 4–6 (`api/field-type`, `api/field`, `api/pricing-rule`).
- Produces: Gateway-proxied endpoints at the same paths, reachable through `http://localhost:82` (Gateway's Kestrel port per `Program.cs`).

- [ ] **Step 1: Add the `booking-cluster` destination** — in `Gateway/Gateway.Api/appsettings.json`, inside `ReverseProxy.Clusters`, add a sibling entry to `administrativeunit-cluster`:

```json
      "booking-cluster": {
        "Destinations": { "destination1": { "Address": "http://localhost:8003/" } }
      }
```

- [ ] **Step 2: Add the read/write-split routes** — each entity gets a public `GET` route and a staff-only mutating route, exactly like `province-read-route`/`province-write-route`. Insert these inside `ReverseProxy.Routes`, right after the existing `administrativeunit-swagger-route` entry:

```json
      "field-type-read-route": {
        "ClusterId": "booking-cluster",
        "Match": { "Path": "/api/field-type/{**catch-all}", "Methods": [ "GET" ] }
      },
      "field-type-write-route": {
        "ClusterId": "booking-cluster",
        "AuthorizationPolicy": "UserOrAdminPolicy",
        "Match": { "Path": "/api/field-type/{**catch-all}", "Methods": [ "POST", "PUT", "DELETE" ] }
      },
      "field-read-route": {
        "ClusterId": "booking-cluster",
        "Match": { "Path": "/api/field/{**catch-all}", "Methods": [ "GET" ] }
      },
      "field-write-route": {
        "ClusterId": "booking-cluster",
        "AuthorizationPolicy": "UserOrAdminPolicy",
        "Match": { "Path": "/api/field/{**catch-all}", "Methods": [ "POST", "PUT", "DELETE" ] }
      },
      "pricing-rule-read-route": {
        "ClusterId": "booking-cluster",
        "Match": { "Path": "/api/pricing-rule/{**catch-all}", "Methods": [ "GET" ] }
      },
      "pricing-rule-write-route": {
        "ClusterId": "booking-cluster",
        "AuthorizationPolicy": "UserOrAdminPolicy",
        "Match": { "Path": "/api/pricing-rule/{**catch-all}", "Methods": [ "POST", "PUT", "DELETE" ] }
      },
      "booking-swagger-route": {
        "ClusterId": "booking-cluster",
        "Match": { "Path": "/booking-swagger/{**catch-all}" },
        "Transforms": [ { "PathPattern": "/swagger/{**catch-all}" } ]
      }
```

- [ ] **Step 3: Add the swagger doc metadata entry** — inside `MicroservicesSwagger.Docs` (array), add a sibling entry to the `administrativeunit-service` one:

```json
      {
        "Name": "booking-service",
        "Title": "Booking API",
        "Url": "/booking-swagger/v1/swagger.json"
      }
```

- [ ] **Step 4: Add the Swagger UI endpoint in `Program.cs`** — inside the `app.UseSwaggerUI(options => { ... })` block, right after the existing `administrativeunit-swagger` line (`options.SwaggerEndpoint("/administrativeunit-swagger/v1/swagger.json", "AdministrativeUnit API");`):

```csharp
        // 4. Cấu hình đọc Swagger JSON của Service Booking thông qua YARP
        options.SwaggerEndpoint("/booking-swagger/v1/swagger.json", "Booking API");
```

- [ ] **Step 5: End-to-end verify through the Gateway**

Run all 3 services (`Gateway.Api`, `Site.Api`, `Booking.Api` — Gateway needs `Site.Api` alive too since `AuthorizationMiddleware`/login live there):

```bash
dotnet run --project Gateway/Gateway.Api/Gateway.Api.csproj &
dotnet run --project Site/Site.Api/Site.Api.csproj &
dotnet run --project Booking/Booking.Api/Booking.Api.csproj &
```

1. Public read through the Gateway (no token):

```bash
curl http://localhost:82/api/field-type
```

Expected: `200 OK`, `Records: []`.

2. Write through the Gateway without a token — must be rejected by the Gateway itself (never reaches `Booking.Api`):

```bash
curl -i -X POST http://localhost:82/api/field-type -H "Content-Type: application/json" -d "{}"
```

Expected: `401 Unauthorized`.

3. Write through the Gateway with a valid staff token (per `.claude/rules/06-testing.md` — login as `admin`/`admin2604` against `POST /api/authorization/login`, use the returned `AccessToken` as `Authorization: Bearer <token>`, plus `X-Device-ID`/`X-Device-Name` headers) — expected to fail with `403 Forbidden` at this stage **unless** a `FieldType` `Function` (`CreateFieldType`) has been created and granted to the test account's Role via the existing Function/Role-Function APIs (same precondition documented in `.claude/rules/06-testing.md` for any new `[RequireFunction]`-gated action). Note this precondition when reporting the test result — it is not a bug in this plan's code.

Stop all 3 processes afterward.

- [ ] **Step 6: Commit**

```bash
git add Gateway/Gateway.Api/appsettings.json Gateway/Gateway.Api/Program.cs
git commit -m "feat: wire Booking service into Gateway (routes, cluster, swagger aggregation)"
```

---

## Self-Review

**Spec coverage** (against [2026-09-01-booking-phase1-design.md](../specs/2026-09-01-booking-phase1-design.md)):
- §2 Kiến trúc tổng quan (service mới, `Field.SiteId`, staff RBAC reuse, public/private route split) → Tasks 1, 2, 5, 7. ✅
- §3.1 `FieldType`, `Field`, `PricingRule` entities → Tasks 4, 5, 6. ✅
- §3.1 `Customer`, `Booking`, `RecurringGroup` entities, §3.3 conflict-check/recurring-generation logic, §4 Customer auth → explicitly deferred to a follow-up plan (stated in Global Constraints and the goal). Not a gap — this plan is scoped to the foundation only, per the writing-plans scope check (a spec covering multiple natural sub-phases should be split; this split was called out to the user before writing began).
- §5 Web nội bộ luồng nghiệp vụ (Admin CRUD sân/giá, Lễ tân xem lịch/xác nhận/walk-in) → the Admin CRUD half is covered (Tasks 4–6); Lễ tân's booking-facing actions depend on the `Booking` entity, deferred with it.
- §6 Response/error handling → every repository method follows `Response<T>`/`AppException`/`[{Entity}_Repo_{Method}-{Number}]` (Tasks 4–6).
- §7 Phạm vi test → this plan's entities are plain CRUD (no conflict-check logic yet), so manual smoke tests are sufficient here; the follow-up plan is where the conflict-check/recurring-generation logic needs deliberate test scenarios.

**Placeholder scan:** no `TBD`/`TODO`/"add appropriate..." phrasing found; every step has literal code or an exact manual command.

**Type consistency check:** `FieldTypeOutputDTO`/`FieldOutputDTO`/`PricingRuleOutputDTO` field names match between the Contract DTOs (Steps 2), the AutoMapper profile (Steps 5), and what the repositories construct/return (Steps 6) in each of Tasks 4–6. `IFieldRepository.UpdateField`/`DeleteField` and `IPricingRuleRepository`'s 3 mutating methods consistently take `callerSiteId` as their last parameter before `CancellationToken`, matching how the Command classes (`CallerSiteId` property) and Controllers (`CurrentSiteId ?? throw ...`) supply it — verified across Tasks 5 and 6.

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-09-01-booking-phase1-foundation-plan.md`. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints

**Which approach?**
