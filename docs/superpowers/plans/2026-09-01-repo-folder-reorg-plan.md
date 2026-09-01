# Repo Folder Reorg by Domain Group Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Group the 3 platform services (`Gateway`, `Site`, `AdministrativeUnit`) under a new `Base/` folder and
the `Booking` service under a new `SportsBooking/` folder (with a `BloodDonation/` placeholder for the next
domain), so the physical folder tree shows at a glance which service belongs to which project — without changing
any project name, namespace, or the 5-layer convention inside each service.

**Architecture:** Pure physical relocation (`git mv`) plus the minimum path fixes required to keep the solution
building: each moved service's `ProjectReference` entries pointing at `CommonLibs` gain one extra `..\` (the
service is now nested one level deeper); `BaseRepo.slnx` and `.vscode/launch.json` get their paths updated to
match; two documentation files get updated to describe the new physical layout.

**Tech Stack:** .NET 10 / MSBuild `.csproj` (`ProjectReference`), `.slnx` solution format, VS Code
`launch.json`, Markdown docs.

**Spec:** [docs/superpowers/specs/2026-09-01-repo-folder-reorg-design.md](../specs/2026-09-01-repo-folder-reorg-design.md)

## Global Constraints

- Do NOT rename any `.csproj` file, any C# namespace, or any class/file inside a moved service. Only physical
  folder location and `ProjectReference`/path strings change.
- Every `ProjectReference` between layers of the *same* service (e.g. `Gateway.Api` → `Gateway.Application`)
  keeps its current relative path unchanged — those two projects move together, so their relative depth to each
  other never changes. Only `ProjectReference` entries pointing at `CommonLibs` change (they gain one `..\`).
- After each service move (Tasks 1–4), verify that specific service's own project builds standalone with
  `dotnet build <path-to-Api-csproj>` before moving to the next task — do not wait until the end to discover a
  missed reference.
- `CommonLibs/`, `Elastic/`, `SystemManagement/`, `docs/` do not move.

---

### Task 1: Move `Gateway` into `Base/Gateway/`

**Files:**
- Move: `Gateway/` → `Base/Gateway/` (git mv, whole tree)
- Modify: `Base/Gateway/Gateway.Contract/Gateway.Contract.csproj:10`
- Modify: `Base/Gateway/Gateway.Domain/Gateway.Domain.csproj:10`
- Modify: `Base/Gateway/Gateway.Api/Gateway.Api.csproj:16`

**Interfaces:**
- Consumes: nothing from other tasks.
- Produces: `Base/Gateway/Gateway.Api/Gateway.Api.csproj` buildable standalone — Task 5 (`BaseRepo.slnx`) and
  Task 6 (`.vscode/launch.json`) reference this new path `Base/Gateway/Gateway.Api/...`.

- [ ] **Step 1: Create the `Base/` folder and move Gateway into it with `git mv`**

```bash
mkdir -p Base
git mv Gateway Base/Gateway
```

- [ ] **Step 2: Verify the move**

Run: `git status --porcelain | head -5`
Expected: a block of `R  Gateway/... -> Base/Gateway/...` rename entries (git detects whole-tree renames
automatically), no lines under `Gateway/` left un-renamed.

- [ ] **Step 3: Fix the `CommonLibs` reference in `Gateway.Contract.csproj`**

In `Base/Gateway/Gateway.Contract/Gateway.Contract.csproj`, change:
```xml
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
```
to:
```xml
    <ProjectReference Include="..\..\..\CommonLibs\CommonLibs.csproj" />
```

- [ ] **Step 4: Fix the `CommonLibs` reference in `Gateway.Domain.csproj`**

In `Base/Gateway/Gateway.Domain/Gateway.Domain.csproj`, apply the identical change as Step 3 (same old/new
string, same line number 10).

- [ ] **Step 5: Fix the `CommonLibs` reference in `Gateway.Api.csproj`**

In `Base/Gateway/Gateway.Api/Gateway.Api.csproj`, change line 16 only (the other 4 `ProjectReference` lines in
this file point at sibling `Gateway.*` projects and must NOT change):
```xml
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
```
to:
```xml
    <ProjectReference Include="..\..\..\CommonLibs\CommonLibs.csproj" />
```

- [ ] **Step 6: Verify `Gateway.Api` builds standalone from its new location**

Run: `dotnet build Base/Gateway/Gateway.Api/Gateway.Api.csproj`
Expected: `Build succeeded. 0 Error(s)` (warnings pre-existing are fine).

- [ ] **Step 7: Commit**

```bash
git add Base/Gateway
git commit -m "refactor: move Gateway into Base/ group, fix CommonLibs references"
```

---

### Task 2: Move `Site` into `Base/Site/`

**Files:**
- Move: `Site/` → `Base/Site/` (git mv, whole tree)
- Modify: `Base/Site/Site.Domain/Site.Domain.csproj:10`
- Modify: `Base/Site/Site.Contract/Site.Contract.csproj:10`
- Modify: `Base/Site/Site.Api/Site.Api.csproj:22`

**Interfaces:**
- Consumes: nothing from Task 1 (independent move).
- Produces: `Base/Site/Site.Api/Site.Api.csproj` buildable standalone — consumed by Task 5/6 same as Task 1.

- [ ] **Step 1: Move Site into `Base/`**

```bash
git mv Site Base/Site
```

- [ ] **Step 2: Verify the move**

Run: `git status --porcelain | head -5`
Expected: `R` rename entries for `Site/... -> Base/Site/...`.

- [ ] **Step 3: Fix the `CommonLibs` reference in `Site.Domain.csproj`**

In `Base/Site/Site.Domain/Site.Domain.csproj`, change:
```xml
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
```
to:
```xml
    <ProjectReference Include="..\..\..\CommonLibs\CommonLibs.csproj" />
```

- [ ] **Step 4: Fix the `CommonLibs` reference in `Site.Contract.csproj`**

In `Base/Site/Site.Contract/Site.Contract.csproj`, apply the identical change as Step 3 (line 10).

- [ ] **Step 5: Fix the `CommonLibs` reference in `Site.Api.csproj`**

In `Base/Site/Site.Api/Site.Api.csproj`, change line 22 only (the other 4 `ProjectReference` lines point at
sibling `Site.*` projects and must NOT change):
```xml
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
```
to:
```xml
    <ProjectReference Include="..\..\..\CommonLibs\CommonLibs.csproj" />
```

- [ ] **Step 6: Verify `Site.Api` builds standalone**

Run: `dotnet build Base/Site/Site.Api/Site.Api.csproj`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 7: Commit**

```bash
git add Base/Site
git commit -m "refactor: move Site into Base/ group, fix CommonLibs references"
```

---

### Task 3: Move `AdministrativeUnit` into `Base/AdministrativeUnit/`

**Files:**
- Move: `AdministrativeUnit/` → `Base/AdministrativeUnit/` (git mv, whole tree)
- Modify: `Base/AdministrativeUnit/AdministrativeUnit.Domain/AdministrativeUnit.Domain.csproj:10`
- Modify: `Base/AdministrativeUnit/AdministrativeUnit.Contract/AdministrativeUnit.Contract.csproj:10`
- Modify: `Base/AdministrativeUnit/AdministrativeUnit.Api/AdministrativeUnit.Api.csproj:22`

**Interfaces:**
- Consumes: nothing from Tasks 1–2 (independent move).
- Produces: `Base/AdministrativeUnit/AdministrativeUnit.Api/AdministrativeUnit.Api.csproj` buildable standalone.

- [ ] **Step 1: Move AdministrativeUnit into `Base/`**

```bash
git mv AdministrativeUnit Base/AdministrativeUnit
```

- [ ] **Step 2: Verify the move**

Run: `git status --porcelain | head -5`
Expected: `R` rename entries for `AdministrativeUnit/... -> Base/AdministrativeUnit/...`.

- [ ] **Step 3: Fix the `CommonLibs` reference in `AdministrativeUnit.Domain.csproj`**

In `Base/AdministrativeUnit/AdministrativeUnit.Domain/AdministrativeUnit.Domain.csproj`, change:
```xml
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
```
to:
```xml
    <ProjectReference Include="..\..\..\CommonLibs\CommonLibs.csproj" />
```

- [ ] **Step 4: Fix the `CommonLibs` reference in `AdministrativeUnit.Contract.csproj`**

In `Base/AdministrativeUnit/AdministrativeUnit.Contract/AdministrativeUnit.Contract.csproj`, apply the
identical change as Step 3 (line 10).

- [ ] **Step 5: Fix the `CommonLibs` reference in `AdministrativeUnit.Api.csproj`**

In `Base/AdministrativeUnit/AdministrativeUnit.Api/AdministrativeUnit.Api.csproj`, change line 22 only (the
other 4 `ProjectReference` lines point at sibling `AdministrativeUnit.*` projects and must NOT change):
```xml
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
```
to:
```xml
    <ProjectReference Include="..\..\..\CommonLibs\CommonLibs.csproj" />
```

- [ ] **Step 6: Verify `AdministrativeUnit.Api` builds standalone**

Run: `dotnet build Base/AdministrativeUnit/AdministrativeUnit.Api/AdministrativeUnit.Api.csproj`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 7: Commit**

```bash
git add Base/AdministrativeUnit
git commit -m "refactor: move AdministrativeUnit into Base/ group, fix CommonLibs references"
```

---

### Task 4: Move `Booking` into `SportsBooking/Booking/`, create `BloodDonation/` placeholder

**Files:**
- Move: `Booking/` → `SportsBooking/Booking/` (git mv, whole tree)
- Modify: `SportsBooking/Booking/Booking.Api/Booking.Api.csproj:22`
- Modify: `SportsBooking/Booking/Booking.Contract/Booking.Contract.csproj:10`
- Modify: `SportsBooking/Booking/Booking.Domain/Booking.Domain.csproj:10`
- Create: `BloodDonation/.gitkeep`

**Interfaces:**
- Consumes: nothing from Tasks 1–3 (independent move).
- Produces: `SportsBooking/Booking/Booking.Api/Booking.Api.csproj` buildable standalone; `BloodDonation/` exists
  as an empty tracked folder for the next domain's future service.

- [ ] **Step 1: Create `SportsBooking/` and move Booking into it**

```bash
mkdir -p SportsBooking
git mv Booking SportsBooking/Booking
```

- [ ] **Step 2: Verify the move**

Run: `git status --porcelain | head -5`
Expected: `R` rename entries for `Booking/... -> SportsBooking/Booking/...`.

- [ ] **Step 3: Fix the `CommonLibs` reference in `Booking.Api.csproj`**

In `SportsBooking/Booking/Booking.Api/Booking.Api.csproj`, change line 22 only (the other 4 `ProjectReference`
lines point at sibling `Booking.*` projects and must NOT change):
```xml
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
```
to:
```xml
    <ProjectReference Include="..\..\..\CommonLibs\CommonLibs.csproj" />
```

- [ ] **Step 4: Fix the `CommonLibs` reference in `Booking.Contract.csproj`**

In `SportsBooking/Booking/Booking.Contract/Booking.Contract.csproj`, change:
```xml
    <ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
```
to:
```xml
    <ProjectReference Include="..\..\..\CommonLibs\CommonLibs.csproj" />
```

- [ ] **Step 5: Fix the `CommonLibs` reference in `Booking.Domain.csproj`**

In `SportsBooking/Booking/Booking.Domain/Booking.Domain.csproj`, apply the identical change as Step 4 (line 10).

- [ ] **Step 6: Verify `Booking.Api` builds standalone**

Run: `dotnet build SportsBooking/Booking/Booking.Api/Booking.Api.csproj`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 7: Create the `BloodDonation/` placeholder folder**

Git does not track empty folders, so add a placeholder file:

```bash
mkdir -p BloodDonation
touch BloodDonation/.gitkeep
```

- [ ] **Step 8: Commit**

```bash
git add SportsBooking BloodDonation
git commit -m "refactor: move Booking into SportsBooking/ group, add BloodDonation/ placeholder"
```

---

### Task 5: Update `BaseRepo.slnx`

**Files:**
- Modify: `BaseRepo.slnx` (full rewrite — 37 lines)

**Interfaces:**
- Consumes: the new paths produced by Tasks 1–4 (`Base/Gateway/...`, `Base/Site/...`,
  `Base/AdministrativeUnit/...`, `SportsBooking/Booking/...`).
- Produces: a solution file `dotnet build BaseRepo.slnx` (Task 7) can restore/build against.

- [ ] **Step 1: Replace the entire content of `BaseRepo.slnx`**

Current content (all `<Project Path>` entries point at the old flat locations):
```xml
<Solution>
  <Folder Name="/AdministrativeUnit/">
    <Project Path="AdministrativeUnit/AdministrativeUnit.Application/AdministrativeUnit.Application.csproj" />
    <Project Path="AdministrativeUnit/AdministrativeUnit.Contract/AdministrativeUnit.Contract.csproj" />
    <Project Path="AdministrativeUnit/AdministrativeUnit.Domain/AdministrativeUnit.Domain.csproj" />
    <Project Path="AdministrativeUnit/AdministrativeUnit.Infrastructure/AdministrativeUnit.Infrastructure.csproj" />
    <Project Path="AdministrativeUnit/AdministrativeUnit.Api/AdministrativeUnit.Api.csproj" />
  </Folder>
  <Folder Name="/Booking/">
    <Project Path="Booking/Booking.Application/Booking.Application.csproj" />
    <Project Path="Booking/Booking.Contract/Booking.Contract.csproj" />
    <Project Path="Booking/Booking.Domain/Booking.Domain.csproj" />
    <Project Path="Booking/Booking.Infrastructure/Booking.Infrastructure.csproj" />
    <Project Path="Booking/Booking.Api/Booking.Api.csproj" />
  </Folder>
  <Folder Name="/CommonLibs/">
    <Project Path="CommonLibs/CommonLibs.csproj" />
  </Folder>
  <Folder Name="/Gateway/">
    <Project Path="Gateway/Gateway.Application/Gateway.Application.csproj" />
    <Project Path="Gateway/Gateway.Contract/Gateway.Contract.csproj" />
    <Project Path="Gateway/Gateway.Domain/Gateway.Domain.csproj" />
    <Project Path="Gateway/Gateway.Infrastructure/Gateway.Infrastructure.csproj" />
    <Project Path="Gateway/Gateway.Api/Gateway.Api.csproj" />
  </Folder>
  <Folder Name="/Site/">
    <Project Path="Site/Site.Application/Site.Application.csproj" />
    <Project Path="Site/Site.Contract/Site.Contract.csproj" />
    <Project Path="Site/Site.Domain/Site.Domain.csproj" />
    <Project Path="Site/Site.Infrastructure/Site.Infrastructure.csproj" />
    <Project Path="Site/Site.Api/Site.Api.csproj" />
  </Folder>
  <Folder Name="/SystemManagement/">
    <Project Path="SystemManagement/SystemManagement.csproj" Id="29c9c436-546e-46a5-8f97-348315652019" />
  </Folder>
</Solution>
```

New content (nested `Base/` and `SportsBooking/` solution folders matching the new physical paths):
```xml
<Solution>
  <Folder Name="/Base/">
    <Folder Name="/Base/AdministrativeUnit/">
      <Project Path="Base/AdministrativeUnit/AdministrativeUnit.Application/AdministrativeUnit.Application.csproj" />
      <Project Path="Base/AdministrativeUnit/AdministrativeUnit.Contract/AdministrativeUnit.Contract.csproj" />
      <Project Path="Base/AdministrativeUnit/AdministrativeUnit.Domain/AdministrativeUnit.Domain.csproj" />
      <Project Path="Base/AdministrativeUnit/AdministrativeUnit.Infrastructure/AdministrativeUnit.Infrastructure.csproj" />
      <Project Path="Base/AdministrativeUnit/AdministrativeUnit.Api/AdministrativeUnit.Api.csproj" />
    </Folder>
    <Folder Name="/Base/Gateway/">
      <Project Path="Base/Gateway/Gateway.Application/Gateway.Application.csproj" />
      <Project Path="Base/Gateway/Gateway.Contract/Gateway.Contract.csproj" />
      <Project Path="Base/Gateway/Gateway.Domain/Gateway.Domain.csproj" />
      <Project Path="Base/Gateway/Gateway.Infrastructure/Gateway.Infrastructure.csproj" />
      <Project Path="Base/Gateway/Gateway.Api/Gateway.Api.csproj" />
    </Folder>
    <Folder Name="/Base/Site/">
      <Project Path="Base/Site/Site.Application/Site.Application.csproj" />
      <Project Path="Base/Site/Site.Contract/Site.Contract.csproj" />
      <Project Path="Base/Site/Site.Domain/Site.Domain.csproj" />
      <Project Path="Base/Site/Site.Infrastructure/Site.Infrastructure.csproj" />
      <Project Path="Base/Site/Site.Api/Site.Api.csproj" />
    </Folder>
  </Folder>
  <Folder Name="/SportsBooking/">
    <Folder Name="/SportsBooking/Booking/">
      <Project Path="SportsBooking/Booking/Booking.Application/Booking.Application.csproj" />
      <Project Path="SportsBooking/Booking/Booking.Contract/Booking.Contract.csproj" />
      <Project Path="SportsBooking/Booking/Booking.Domain/Booking.Domain.csproj" />
      <Project Path="SportsBooking/Booking/Booking.Infrastructure/Booking.Infrastructure.csproj" />
      <Project Path="SportsBooking/Booking/Booking.Api/Booking.Api.csproj" />
    </Folder>
  </Folder>
  <Folder Name="/CommonLibs/">
    <Project Path="CommonLibs/CommonLibs.csproj" />
  </Folder>
  <Folder Name="/SystemManagement/">
    <Project Path="SystemManagement/SystemManagement.csproj" Id="29c9c436-546e-46a5-8f97-348315652019" />
  </Folder>
</Solution>
```

- [ ] **Step 2: Verify the solution file parses and lists all 21 projects**

Run: `dotnet sln BaseRepo.slnx list`
Expected: 21 project paths listed (5 AdministrativeUnit + 5 Gateway + 5 Site + 5 Booking + 1 CommonLibs + 1
SystemManagement — `BloodDonation/` has no project yet, correctly absent from this list), all under
`Base/`/`SportsBooking/` as appropriate, none under the old flat paths.

- [ ] **Step 3: Commit**

```bash
git add BaseRepo.slnx
git commit -m "refactor: update BaseRepo.slnx paths for the new Base/SportsBooking grouping"
```

---

### Task 6: Update `.vscode/launch.json`

**Files:**
- Modify: `.vscode/launch.json`

**Interfaces:**
- Consumes: the new paths produced by Task 1 (`Base/Gateway/...`), Task 2 (`Base/Site/...`), Task 3
  (`Base/AdministrativeUnit/...`).
- Produces: nothing consumed by later tasks (VS Code debug config only).

- [ ] **Step 1: Update the Gateway launch config**

In `.vscode/launch.json`, change:
```json
            "program": "${workspaceFolder}/Gateway/Gateway.Api/bin/Debug/net10.0/Gateway.Api.dll",
            "args": [],
            "cwd": "${workspaceFolder}/Gateway/Gateway.Api",
```
to:
```json
            "program": "${workspaceFolder}/Base/Gateway/Gateway.Api/bin/Debug/net10.0/Gateway.Api.dll",
            "args": [],
            "cwd": "${workspaceFolder}/Base/Gateway/Gateway.Api",
```

- [ ] **Step 2: Update the Site launch config**

In the same file, change:
```json
            "program": "${workspaceFolder}/Site/Site.Api/bin/Debug/net10.0/Site.Api.dll",
            "args": [],
            "cwd": "${workspaceFolder}/Site/Site.Api",
```
to:
```json
            "program": "${workspaceFolder}/Base/Site/Site.Api/bin/Debug/net10.0/Site.Api.dll",
            "args": [],
            "cwd": "${workspaceFolder}/Base/Site/Site.Api",
```

- [ ] **Step 3: Update the AdministrativeUnit launch config**

In the same file, change:
```json
            "program": "${workspaceFolder}/AdministrativeUnit/AdministrativeUnit.Api/bin/Debug/net10.0/AdministrativeUnit.Api.dll",
            "args": [],
            "cwd": "${workspaceFolder}/AdministrativeUnit/AdministrativeUnit.Api",
```
to:
```json
            "program": "${workspaceFolder}/Base/AdministrativeUnit/AdministrativeUnit.Api/bin/Debug/net10.0/AdministrativeUnit.Api.dll",
            "args": [],
            "cwd": "${workspaceFolder}/Base/AdministrativeUnit/AdministrativeUnit.Api",
```

- [ ] **Step 4: Verify the JSON is still well-formed**

Run: `node -e "JSON.parse(require('fs').readFileSync('.vscode/launch.json','utf8')); console.log('OK')"`
Expected: prints `OK` (no `SyntaxError`).

- [ ] **Step 5: Commit**

```bash
git add .vscode/launch.json
git commit -m "chore: update .vscode/launch.json paths for the new Base/ grouping"
```

---

### Task 7: Full solution build verification

**Files:**
- None modified — verification-only task.

**Interfaces:**
- Consumes: the completed, committed state of Tasks 1–6.
- Produces: a confirmed-green baseline that Task 8's documentation describes accurately.

- [ ] **Step 1: Clean restore and build the whole solution**

Run: `dotnet restore BaseRepo.slnx && dotnet build BaseRepo.slnx`
Expected: `Build succeeded.` with `0 Error(s)`. Warning count should match the pre-reorg baseline (no new
warnings introduced by path changes — if new warnings about missing files appear, a reference was missed in
Tasks 1–4).

- [ ] **Step 2: Confirm no old flat-path folders remain**

Run: `ls Gateway Site AdministrativeUnit Booking 2>&1 || true`
Expected: all four report "No such file or directory" (or the PowerShell/Windows equivalent) — confirms the
`git mv` fully relocated each tree and nothing was left behind at the old path.

- [ ] **Step 3: No commit needed** — this task only verifies; Tasks 1–6 already committed the actual changes.

---

### Task 8: Update documentation to describe the new physical layout

**Files:**
- Modify: `claude.md:28-187` (top-level folder tree)
- Modify: `claude.md:247-260` ("Cấu trúc thư mục vật lý" section)
- Modify: `.claude/rules/01-architecture.md:43-45`

**Interfaces:**
- Consumes: nothing (pure documentation, no code dependency).
- Produces: nothing consumed by other tasks — this is the last task.

- [ ] **Step 1: Rewrite the top-level folder tree in `claude.md`**

Replace the block starting at `## 📁 Cấu trúc thư mục tổng quan` (current content shown below, lines 28-187)
with the version below that reflects `Base/`/`SportsBooking/`/`BloodDonation/` and adds `Booking` (which was
missing from this diagram even before the reorg).

Current content — the full fenced block between the two ` ``` ` markers, starting `BaseRepo/` and ending with
the `SystemManagement` sub-tree (this is the exact block to replace, reproduced here in full so the diff is
unambiguous):
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
├── Gateway/                          # 🌐 Service Gateway — 5 project con gom vào 1 folder cha (pilot cấu trúc mới,
│   │                                 #    xem "Cấu trúc project (pilot Gateway)" bên dưới; Site/AdministrativeUnit
│   │                                 #    vẫn ở dạng flat cũ, CHƯA áp dụng cấu trúc này)
│   ├── Gateway.Api/                  # 🌐 API Gateway (Presentation Layer)
│   │   ├── Controllers/
│   │   │   ├── AuthorizationController.cs
│   │   │   ├── UserController.cs
│   │   │   └── TestGatewayAPIController.cs
│   │   ├── Utils/
│   │   ├── Keys/                     # JWT signing keys
│   │   ├── Program.cs                # Entry point + DI + YARP + Auth config
│   │   ├── appsettings.json          # Config (YARP routes, DB, JWT, Consul...)
│   │   └── log4net.config
│   │
│   ├── Gateway.Application/          # 📋 Application Layer (CQRS Handlers)
│   │   ├── Features/                 # Feature-based organization
│   │   │   ├── Authorization/
│   │   │   │   ├── Login/
│   │   │   │   │   ├── LoginCommand.cs
│   │   │   │   │   └── LoginCommandHandler.cs
│   │   │   │   └── Logout/
│   │   │   │       ├── LogoutCommand.cs
│   │   │   │       └── LogoutCommandHandler.cs
│   │   │   └── User/
│   │   │       ├── CreateUser/
│   │   │       │   ├── CreateUserCommand.cs
│   │   │       │   └── CreateUserCommandHandler.cs
│   │   │       ├── GetAllUsers/
│   │   │       │   ├── GetAllUsersQuery.cs
│   │   │       │   └── GetAllUsersQueryHandler.cs
│   │   │       └── GetUserById/
│   │   │           ├── GetUserByIdQuery.cs
│   │   │           └── GetUserByIdQueryHandler.cs
│   │   ├── Interfaces/               # Repository interfaces
│   │   │   ├── IAuthorizationRepository.cs
│   │   │   └── IUserRepository.cs
│   │   ├── Mappings/                 # AutoMapper profiles
│   │   │   └── GatewayMapping.cs
│   │   └── Middlewares/
│   │       └── AuthorizationMiddleware.cs
│   │
│   ├── Gateway.Contract/             # 📦 Contract Layer (DTOs — tách khỏi Domain)
│   │   └── DTOs/
│   │       ├── AuthorizationDTO.cs
│   │       └── UserDTO.cs           # UserInputDTO, UserOutputDTO, UserFilterDTO
│   │
│   ├── Gateway.Domain/               # 🏛️ Domain Layer (chỉ còn Entities — DTO đã chuyển sang Gateway.Contract)
│   │   └── Entities/
│   │       └── User.cs               # Rich Domain Entity với guard methods
│   │
│   └── Gateway.Infrastructure/       # 🔌 Infrastructure Layer (Data Access)
│       ├── DBContext/
│       │   └── GatewayDbContext.cs   # DbContext kế thừa BaseContext
│       ├── Repositories/
│       │   ├── AuthorizationRepository.cs
│       │   └── UserRepository.cs    # Implement IUserRepository
│       └── DependencyInjection.cs   # Extension method đăng ký DI
│
├── Site/                              # 🏥 Service Site — cùng khuôn 5 project như Gateway
│   ├── Site.Api/                      # Presentation Layer
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Serilog.json   # Cấu hình Serilog tách riêng, xem "Tách appsettings.Serilog.json" bên dưới
│   ├── Site.Application/              # Application Layer
│   │   ├── Features/                  # (cấu trúc tương tự Gateway.Application)
│   │   ├── Interfaces/
│   │   │   ├── ISiteRepository.cs
│   │   │   ├── IModuleRepository.cs
│   │   │   ├── IRoleRepository.cs
│   │   │   ├── IScreenRepository.cs
│   │   │   ├── IFunctionRepository.cs
│   │   │   └── ...
│   │   └── Mappings/
│   │       └── SiteMapping.cs
│   ├── Site.Contract/                 # Contract Layer (DTOs)
│   │   └── DTOs/
│   │       ├── SiteDTO.cs
│   │       ├── ModuleDTO.cs
│   │       ├── RoleDTO.cs
│   │       ├── ScreenDTO.cs
│   │       ├── FunctionDTO.cs
│   │       └── ...
│   ├── Site.Domain/                   # Domain Layer (chỉ Entities)
│   │   └── Entities/
│   │       ├── Site.cs
│   │       ├── Module.cs
│   │       ├── Role.cs
│   │       ├── Screen.cs
│   │       ├── Function.cs
│   │       └── ...
│   └── Site.Infrastructure/           # Infrastructure Layer
│       ├── DBContext/
│       │   └── SiteDbContext.cs
│       ├── Repositories/
│       │   └── SiteRepository.cs
│       └── DependencyInjection.cs
│
├── AdministrativeUnit/                # 🗺️ Service AdministrativeUnit — cùng khuôn 5 project (Province + Ward)
│   ├── AdministrativeUnit.Api/
│   ├── AdministrativeUnit.Application/
│   ├── AdministrativeUnit.Contract/
│   ├── AdministrativeUnit.Domain/
│   └── AdministrativeUnit.Infrastructure/
│
├── Elastic/                          # 📊 Elasticsearch + Kibana + Filebeat tools
│
└── SystemManagement/                 # 🖥️ Windows desktop management tool (WinForms) — không thuộc kiến trúc
    ├── Config/                       #    5-layer này, giữ nguyên như cũ
    ├── Interfaces/
    ├── Models/
    └── Views/
```

New content (same fenced block, regrouped under `Base/`/`SportsBooking/`, `Booking` added, `BloodDonation/`
added — sub-trees for `Site`/`AdministrativeUnit`/`Booking` collapsed to their existing shorthand depth since
this diagram never expanded them in detail before either):
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
│   │   │   ├── appsettings.json      # Config (YARP routes, DB, JWT, Consul...)
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

- [ ] **Step 2: Update the "Cấu trúc thư mục vật lý" section**

In `claude.md`, find the section starting `### Cấu trúc thư mục vật lý` (current content shown below) and
replace it entirely:

Current content:
```
### Cấu trúc thư mục vật lý

Cả 5 project của 1 service được gom vào 1 folder cha cùng tên service — tên project/namespace của
Application/Contract/Domain/Infrastructure giữ nguyên hậu tố `.Application`/`.Contract`/`.Domain`/`.Infrastructure`
như cũ; riêng Presentation đổi tên từ bare `{ServiceName}` thành `{ServiceName}.Api` (namespace
`{ServiceName}.Controllers`\`{ServiceName}.Utils` → `{ServiceName}.Api.Controllers`\`{ServiceName}.Api.Utils`):
```
{ServiceName}/
├── {ServiceName}.Api/                (Presentation)
├── {ServiceName}.Application/
├── {ServiceName}.Contract/
├── {ServiceName}.Domain/
└── {ServiceName}.Infrastructure/
```
```

New content:
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
`{ServiceName}.Controllers`\`{ServiceName}.Utils` → `{ServiceName}.Api.Controllers`\`{ServiceName}.Api.Utils`):
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
```

- [ ] **Step 3: Update `.claude/rules/01-architecture.md`**

In `.claude/rules/01-architecture.md`, change:
```markdown
Project mới phải theo đúng 5 hậu tố layer: `{Service}.Api` (Presentation), `{Service}.Application`,
`{Service}.Contract` (DTO), `{Service}.Domain` (Entity), `{Service}.Infrastructure` — cả 5 gom vào 1 folder cha
`{Service}/` (xem `claude.md` mục "Cấu trúc thư mục vật lý").
```
to:
```markdown
Project mới phải theo đúng 5 hậu tố layer: `{Service}.Api` (Presentation), `{Service}.Application`,
`{Service}.Contract` (DTO), `{Service}.Domain` (Entity), `{Service}.Infrastructure` — cả 5 gom vào 1 folder cha
`{Service}/`, và `{Service}/` nằm trong 1 `{DomainGroup}/` (`Base/` cho hạ tầng dùng chung, hoặc tên domain
nghiệp vụ — VD `SportsBooking/`) — xem `claude.md` mục "Cấu trúc thư mục vật lý".
```

- [ ] **Step 4: Add a Progress Log entry in `claude.md`**

At the top of the `### Đã hoàn thành` list (right after that heading, same convention as every prior entry in
this file), add:
```markdown
- **Tái cấu trúc thư mục repo theo nhóm domain** (2026-09-01) — gom `Gateway`/`Site`/`AdministrativeUnit` vào
  `Base/`, `Booking` vào `SportsBooking/`, thêm `BloodDonation/` làm chỗ chứa domain kế tiếp. Không đổi tên
  project/namespace nào, chỉ dời vị trí vật lý (`git mv`, giữ lịch sử) + sửa `ProjectReference` trỏ
  `CommonLibs` (thêm 1 cấp `..\`) + `BaseRepo.slnx` + `.vscode/launch.json`. Spec:
  `docs/superpowers/specs/2026-09-01-repo-folder-reorg-design.md`, plan:
  `docs/superpowers/plans/2026-09-01-repo-folder-reorg-plan.md`. `dotnet build BaseRepo.slnx`: 0 Error.
```

- [ ] **Step 5: Commit**

```bash
git add claude.md .claude/rules/01-architecture.md
git commit -m "docs: describe the new Base/SportsBooking/BloodDonation folder grouping"
```

---

## Self-Review

- **Spec coverage**: spec section 3 (target tree) → Tasks 1–4 + Task 5's new `.slnx`; spec section 4.1 (git mv)
  → Tasks 1–4 Step 1; spec section 4.2 (path fixes) → Tasks 1–4 Steps 3–5, Task 5, Task 6; spec section 4.3
  (skills verified, no change needed) → correctly NOT a task in this plan; spec section 5 (docs) → Task 8; spec
  section 7 (test scope) → Task 7.
- **Placeholder scan**: no TBD/TODO; every step shows the literal old/new string or exact command.
- **Type consistency**: n/a (no code types involved, only paths — verified every path string above against the
  actual repo content via `Grep`/`Read`, not assumed).

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-09-01-repo-folder-reorg-plan.md`. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints

**Which approach?**
