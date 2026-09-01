# SQL File Relocation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move every `.sql` install/seed file out of the centralized `CommonLibs/BaseSQL/` folder into a
`SQL/` subfolder inside the `.Api` project of the service that owns it, so each service is
self-contained and a future tool (SystemManagement's `UC_ConfigSQL`, out of scope here) can find a
service's SQL files via that service's own deployed folder path.

**Architecture:** Pure file relocation (`git mv`) plus the minimum path-reference fixes inside the
moved files and in docs that point at the old location — no SQL content changes, no C# code changes.
Each `.Api.csproj` gets a `Content Include="SQL\**"` entry so the files copy to the build/publish
output alongside `appsettings.json`/`log4net.config`.

**Tech Stack:** MSBuild `.csproj` (`Content Include`), plain file moves, Markdown/SQL comment edits.

**Spec:** [docs/superpowers/specs/2026-09-02-sql-file-relocation-and-system-management-sql-tool-design.md](../specs/2026-09-02-sql-file-relocation-and-system-management-sql-tool-design.md)
— this plan implements ONLY section 2 "Đợt này" (file relocation) and the relevant parts of section 3.
Section 5-6 (the `UC_ConfigSQL` SystemManagement screen) is explicitly out of scope for this plan.

## Global Constraints

- Do NOT move `CommonLibs/BaseSQL/multi-tenant/` (README.md or `tests/`) — stays exactly where it is.
- Do NOT change any SQL statement, table, or column in any moved file — only relocate + fix path
  references inside comments/docs.
- Do NOT touch `Site.Api` — it has no `SQL/` folder of its own (it shares `Gateway.Api`'s `BaseSQL.sql`).
- Every moved file keeps its exact filename (no renaming).
- After each task, verify the affected `.Api` project builds AND the `.sql`/`.md` files actually appear
  in that project's `bin/Debug/net10.0/SQL/` output — a `Content Include` that doesn't match real files
  fails silently (0 errors, but nothing copied), so this must be checked explicitly, not assumed from a
  clean build.

---

### Task 1: Move Gateway's SQL files into `Base/Gateway/Gateway.Api/SQL/`

**Files:**
- Move: `CommonLibs/BaseSQL/BaseSQL.sql` → `Base/Gateway/Gateway.Api/SQL/BaseSQL.sql`
- Move: `CommonLibs/BaseSQL/seed_admin_account.sql` → `Base/Gateway/Gateway.Api/SQL/seed_admin_account.sql`
- Modify: `Base/Gateway/Gateway.Api/Gateway.Api.csproj`

**Interfaces:**
- Consumes: nothing from other tasks.
- Produces: `Base/Gateway/Gateway.Api/SQL/BaseSQL.sql` and `.../seed_admin_account.sql` — Task 2's
  `BaseAdministrativeUnit.sql`/`seed_administrative_unit.sql` reference these two files by their new
  path in comments (fixed in Task 2, not here).

- [ ] **Step 1: Move the 2 files with `git mv`**

```bash
mkdir -p Base/Gateway/Gateway.Api/SQL
git mv CommonLibs/BaseSQL/BaseSQL.sql Base/Gateway/Gateway.Api/SQL/BaseSQL.sql
git mv CommonLibs/BaseSQL/seed_admin_account.sql Base/Gateway/Gateway.Api/SQL/seed_admin_account.sql
```

- [ ] **Step 2: Verify the move**

Run: `git status --porcelain | head -5`
Expected: `R  CommonLibs/BaseSQL/BaseSQL.sql -> Base/Gateway/Gateway.Api/SQL/BaseSQL.sql` and the same for
`seed_admin_account.sql` (git detects the rename automatically).

- [ ] **Step 3: Add the `Content Include` to `Gateway.Api.csproj`**

In `Base/Gateway/Gateway.Api/Gateway.Api.csproj`, change:
```xml
  <ItemGroup>
    <Folder Include="Logs\" />
  </ItemGroup>

</Project>
```
to:
```xml
  <ItemGroup>
    <Folder Include="Logs\" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="SQL\**" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

</Project>
```

- [ ] **Step 4: Build and verify the files land in the output directory**

Run: `dotnet build Base/Gateway/Gateway.Api/Gateway.Api.csproj`
Expected: `Build succeeded. 0 Error(s)`.

Run: `ls Base/Gateway/Gateway.Api/bin/Debug/net10.0/SQL/`
Expected: lists both `BaseSQL.sql` and `seed_admin_account.sql`.

- [ ] **Step 5: Commit**

```bash
git add Base/Gateway/Gateway.Api/SQL Base/Gateway/Gateway.Api/Gateway.Api.csproj
git commit -m "refactor: move Gateway's SQL files into Gateway.Api/SQL/"
```

---

### Task 2: Move AdministrativeUnit's SQL files into `Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/`

**Files:**
- Move: `CommonLibs/BaseSQL/administrative-unit/BaseAdministrativeUnit.sql` → `Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/BaseAdministrativeUnit.sql`
- Move: `CommonLibs/BaseSQL/administrative-unit/seed_administrative_unit.sql` → `Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/seed_administrative_unit.sql`
- Move: `CommonLibs/BaseSQL/administrative-unit/README.md` → `Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/README.md`
- Modify: `Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/BaseAdministrativeUnit.sql` (3 path-reference fixes)
- Modify: `Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/seed_administrative_unit.sql` (1 path-reference fix)
- Modify: `Base/AdministrativeUnit/AdministrativeUnit.Api/AdministrativeUnit.Api.csproj`

**Interfaces:**
- Consumes: `Base/Gateway/Gateway.Api/SQL/BaseSQL.sql` and `.../seed_admin_account.sql` paths produced
  by Task 1 (referenced in comments fixed at Step 4 below).
- Produces: nothing consumed by later tasks.

- [ ] **Step 1: Create the target folder and move the 3 files with `git mv`**

```bash
mkdir -p Base/AdministrativeUnit/AdministrativeUnit.Api/SQL
git mv CommonLibs/BaseSQL/administrative-unit/BaseAdministrativeUnit.sql Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/BaseAdministrativeUnit.sql
git mv CommonLibs/BaseSQL/administrative-unit/seed_administrative_unit.sql Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/seed_administrative_unit.sql
git mv CommonLibs/BaseSQL/administrative-unit/README.md Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/README.md
```

- [ ] **Step 2: Verify the move**

Run: `git status --porcelain | head -5`
Expected: 3 `R` rename entries into `Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/`.

- [ ] **Step 3: Verify the old `administrative-unit/` folder is now empty and remove it**

Run: `ls CommonLibs/BaseSQL/administrative-unit/ 2>&1`
Expected: "No such file or directory" or empty listing (git does not track empty directories, so once
all 3 files are moved the directory itself disappears — if `ls` still shows it as an existing empty
directory on disk, run `rmdir CommonLibs/BaseSQL/administrative-unit` to clean it up; this is a no-op if
git already removed it).

- [ ] **Step 4: Fix the 3 path references inside `BaseAdministrativeUnit.sql`**

In `Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/BaseAdministrativeUnit.sql`, change:
```sql
-- Sau khi chạy xong file này, seed dữ liệu 34 Province + 3.321 Ward bằng
-- CommonLibs/BaseSQL/administrative-unit/seed_administrative_unit.sql (xem README.md cùng thư mục).
```
to:
```sql
-- Sau khi chạy xong file này, seed dữ liệu 34 Province + 3.321 Ward bằng
-- seed_administrative_unit.sql (cùng thư mục này, xem README.md cùng thư mục).
```

Then, in the same file, there are 2 identical occurrences of this line (in the `Province` table
comment and the `Ward` table comment) — replace **both**:
```sql
    -- Giống mọi bảng khác trong CommonLibs/BaseSQL/BaseSQL.sql: AutoId là PRIMARY KEY (clustered),
```
with:
```sql
    -- Giống mọi bảng khác trong Base/Gateway/Gateway.Api/SQL/BaseSQL.sql: AutoId là PRIMARY KEY (clustered),
```
(Use `replace_all` / a global find-replace within this file for this second pair — both occurrences get
the identical new text.)

- [ ] **Step 5: Fix the 1 path reference inside `seed_administrative_unit.sql`**

In `Base/AdministrativeUnit/AdministrativeUnit.Api/SQL/seed_administrative_unit.sql`, change:
```sql
  thành 1 file duy nhất, cùng phong cách với CommonLibs/BaseSQL/seed_admin_account.sql.
```
to:
```sql
  thành 1 file duy nhất, cùng phong cách với Base/Gateway/Gateway.Api/SQL/seed_admin_account.sql.
```

- [ ] **Step 6: Add the `Content Include` to `AdministrativeUnit.Api.csproj`**

In `Base/AdministrativeUnit/AdministrativeUnit.Api/AdministrativeUnit.Api.csproj`, change:
```xml
  <ItemGroup>
    <Folder Include="Controllers\" />
  </ItemGroup>

</Project>
```
to:
```xml
  <ItemGroup>
    <Folder Include="Controllers\" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="SQL\**" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

</Project>
```

- [ ] **Step 7: Build and verify the files land in the output directory**

Run: `dotnet build Base/AdministrativeUnit/AdministrativeUnit.Api/AdministrativeUnit.Api.csproj`
Expected: `Build succeeded. 0 Error(s)`.

Run: `ls Base/AdministrativeUnit/AdministrativeUnit.Api/bin/Debug/net10.0/SQL/`
Expected: lists `BaseAdministrativeUnit.sql`, `seed_administrative_unit.sql`, and `README.md`.

- [ ] **Step 8: Commit**

```bash
git add Base/AdministrativeUnit/AdministrativeUnit.Api/SQL Base/AdministrativeUnit/AdministrativeUnit.Api/AdministrativeUnit.Api.csproj
git add -u CommonLibs/BaseSQL/administrative-unit
git commit -m "refactor: move AdministrativeUnit's SQL files into AdministrativeUnit.Api/SQL/, fix internal path references"
```

---

### Task 3: Move Booking's SQL file into `SportsBooking/Booking/Booking.Api/SQL/`

**Files:**
- Move: `CommonLibs/BaseSQL/booking/BaseBooking.sql` → `SportsBooking/Booking/Booking.Api/SQL/BaseBooking.sql`
- Modify: `SportsBooking/Booking/Booking.Api/Booking.Api.csproj`

**Interfaces:**
- Consumes: nothing from Tasks 1–2 (independent move; `BaseBooking.sql` has no path references to fix —
  confirmed via grep, it contains zero mentions of `CommonLibs/BaseSQL`).
- Produces: nothing consumed by later tasks.

- [ ] **Step 1: Create the target folder and move the file with `git mv`**

```bash
mkdir -p SportsBooking/Booking/Booking.Api/SQL
git mv CommonLibs/BaseSQL/booking/BaseBooking.sql SportsBooking/Booking/Booking.Api/SQL/BaseBooking.sql
```

- [ ] **Step 2: Verify the move and that the old `booking/` folder is gone**

Run: `git status --porcelain | head -3`
Expected: 1 `R` rename entry into `SportsBooking/Booking/Booking.Api/SQL/`.

Run: `ls CommonLibs/BaseSQL/booking/ 2>&1`
Expected: "No such file or directory" (only file in that folder was `BaseBooking.sql`, so the directory
disappears once it's moved — if it still shows as an empty directory on disk, run
`rmdir CommonLibs/BaseSQL/booking` to clean it up).

- [ ] **Step 3: Add the `Content Include` to `Booking.Api.csproj`**

In `SportsBooking/Booking/Booking.Api/Booking.Api.csproj`, change:
```xml
  <ItemGroup>
    <Folder Include="Controllers\" />
  </ItemGroup>

</Project>
```
to:
```xml
  <ItemGroup>
    <Folder Include="Controllers\" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="SQL\**" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

</Project>
```

- [ ] **Step 4: Build and verify the file lands in the output directory**

Run: `dotnet build SportsBooking/Booking/Booking.Api/Booking.Api.csproj`
Expected: `Build succeeded. 0 Error(s)`.

Run: `ls SportsBooking/Booking/Booking.Api/bin/Debug/net10.0/SQL/`
Expected: lists `BaseBooking.sql`.

- [ ] **Step 5: Commit**

```bash
git add SportsBooking/Booking/Booking.Api/SQL SportsBooking/Booking/Booking.Api/Booking.Api.csproj
git add -u CommonLibs/BaseSQL/booking
git commit -m "refactor: move Booking's SQL file into Booking.Api/SQL/"
```

---

### Task 4: Update remaining docs that reference the old `CommonLibs/BaseSQL/...` paths

**Files:**
- Modify: `Base/AdministrativeUnit/AdministrativeUnit.Api/appsettings.json` (1 comment line)
- Modify: `CommonLibs/BaseSQL/multi-tenant/README.md` (4 lines)
- Modify: `claude.md` (Progress Log entry)

**Interfaces:**
- Consumes: the final paths produced by Tasks 1–3.
- Produces: nothing consumed by other tasks — this is the last content task.

- [ ] **Step 1: Fix the comment in `AdministrativeUnit.Api/appsettings.json`**

In `Base/AdministrativeUnit/AdministrativeUnit.Api/appsettings.json`, change:
```json
  // Cấu hình kết nối đến SQL Server. Lưu ý: dùng chung SQL Server instance + login với
  // Site/Gateway (localhost,1433), chỉ khác Database (schema riêng "AdministrativeUnit" theo
  // CommonLibs/BaseSQL/administrative-unit/BaseAdministrativeUnit.sql) - nếu login DB thực tế
  // khác, cần cập nhật lại UserName/Password đã mã hoá tại đây.
```
to:
```json
  // Cấu hình kết nối đến SQL Server. Lưu ý: dùng chung SQL Server instance + login với
  // Site/Gateway (localhost,1433), chỉ khác Database (schema riêng "AdministrativeUnit" theo
  // SQL/BaseAdministrativeUnit.sql cùng project này) - nếu login DB thực tế
  // khác, cần cập nhật lại UserName/Password đã mã hoá tại đây.
```

- [ ] **Step 2: Fix the 4 references in `CommonLibs/BaseSQL/multi-tenant/README.md`**

In `CommonLibs/BaseSQL/multi-tenant/README.md`, change (first occurrence, in the "Bối cảnh" section):
```markdown
Không có server nào chạy sẵn để test các script này — đã rà logic kỹ và đối chiếu đúng
tên bảng/cột/index trong `CommonLibs/BaseSQL/BaseSQL.sql`, nhưng **bắt buộc chạy thử trên
```
to:
```markdown
Không có server nào chạy sẵn để test các script này — đã rà logic kỹ và đối chiếu đúng
tên bảng/cột/index trong `Base/Gateway/Gateway.Api/SQL/BaseSQL.sql`, nhưng **bắt buộc chạy thử trên
```

Then change (in the "Cách chạy" section):
```markdown
`CommonLibs/BaseSQL/BaseSQL.sql` — chỉ cần chạy file đó (kèm `CommonLibs/BaseSQL/seed_admin_account.sql`
```
to:
```markdown
`Base/Gateway/Gateway.Api/SQL/BaseSQL.sql` — chỉ cần chạy file đó (kèm `Base/Gateway/Gateway.Api/SQL/seed_admin_account.sql`
```

Then change (in the "Fix: RoleId vẫn unique toàn cục" section):
```markdown
`(SiteId, RoleId)` trên 1 DB đã tồn tại. Với DB mới, `CommonLibs/BaseSQL/BaseSQL.sql` đã khai
```
to:
```markdown
`(SiteId, RoleId)` trên 1 DB đã tồn tại. Với DB mới, `Base/Gateway/Gateway.Api/SQL/BaseSQL.sql` đã khai
```

Then change (in the "Lưu ý / điểm mở" section):
```markdown
- **[2026-09-01]** `CommonLibs/BaseSQL/BaseSQL.sql` đã được viết lại để có đầy đủ
```
to:
```markdown
- **[2026-09-01]** `Base/Gateway/Gateway.Api/SQL/BaseSQL.sql` (đường dẫn kể từ 2026-09-02 — trước đó
  là `CommonLibs/BaseSQL/BaseSQL.sql`, xem Progress Log trong `claude.md`) đã được viết lại để có đầy đủ
```

- [ ] **Step 3: Add a Progress Log entry to `claude.md`**

At the top of the `### Đã hoàn thành` list in `claude.md` (right after that heading — same convention as
every prior entry in this file), add:
```markdown
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
```

- [ ] **Step 4: Commit**

```bash
git add Base/AdministrativeUnit/AdministrativeUnit.Api/appsettings.json CommonLibs/BaseSQL/multi-tenant/README.md claude.md
git commit -m "docs: update references to the relocated SQL files"
```

---

### Task 5: Final whole-solution verification

**Files:**
- None modified — verification-only task.

**Interfaces:**
- Consumes: the completed, committed state of Tasks 1–4.
- Produces: a confirmed-green baseline.

- [ ] **Step 1: Clean build the whole solution**

Run: `dotnet build BaseRepo.slnx`
Expected: `Build succeeded.` with `0 Error(s)`.

- [ ] **Step 2: Confirm no `.sql` files remain directly under the old locations**

Run: `find CommonLibs/BaseSQL -maxdepth 2 -iname "*.sql"`
Expected: only files under `CommonLibs/BaseSQL/multi-tenant/tests/` are listed (`seed_two_site_isolation.sql`,
`cleanup_two_site_isolation.sql`) — `BaseSQL.sql`, `seed_admin_account.sql`, and the
`administrative-unit/`/`booking/` subfolders must NOT appear.

- [ ] **Step 3: Confirm all 3 output `SQL/` folders are populated**

Run:
```bash
ls Base/Gateway/Gateway.Api/bin/Debug/net10.0/SQL/
ls Base/AdministrativeUnit/AdministrativeUnit.Api/bin/Debug/net10.0/SQL/
ls SportsBooking/Booking/Booking.Api/bin/Debug/net10.0/SQL/
```
Expected: 2, 3, and 1 file(s) respectively, matching Tasks 1–3.

- [ ] **Step 4: No commit needed** — this task only verifies; Tasks 1–4 already committed the actual changes.

## Self-Review

- **Spec coverage**: spec section 2 "Đợt này" (relocate files) → Tasks 1–3; spec section 3 (target
  structure, `multi-tenant/` stays) → Tasks 1–3 + Task 5 Step 2; docs consistency → Task 4. Spec
  sections 4–6 (SystemManagement `UC_ConfigSQL`, `DefaultConnection` field) are explicitly out of scope
  for this plan per the spec's own "Đợt sau" split — correctly not a task here.
- **Placeholder scan**: no TBD/TODO; every step shows the literal old/new string, exact command, or
  exact file list.
- **Type consistency**: n/a (no code types involved, only file paths and Markdown/SQL comments —
  verified every path string above against the actual repo content via `Grep`, not assumed).

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-09-02-sql-file-relocation-plan.md`. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints

**Which approach?**
