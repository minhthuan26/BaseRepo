# Migration: Site làm ranh giới tenant (RBAC multi-tenant)

## Bối cảnh

Theo tài liệu thiết kế đã chốt —
[Multi-Tenant RBAC cho BaseRepo](https://claude.ai/code/artifact/287ffb5c-0fc3-4fd0-81ec-a9f61d4e018b)
— hệ thống dùng thẳng `tbl_Site` làm ranh giới tenant, không thêm bảng `Tenant` riêng. Ba
thay đổi cốt lõi trên schema `BaseRepo`:

1. `tbl_Users` gắn thẳng `SiteId` (1 User chỉ thuộc đúng 1 Site), bỏ bảng trung gian
   `tbl_User_Site`. `UserId` đổi từ unique toàn hệ thống sang unique theo từng Site.
2. `tbl_Modules` (và gián tiếp `tbl_Screens`/`tbl_Functions` qua chuỗi quan hệ) trở thành
   danh mục dùng chung toàn hệ thống, không còn gắn `SiteId` — vì tập Function vốn đã
   hardcode theo tên method C#, giống nhau cho mọi Site. Bảng mới `tbl_Site_Module` đảm
   nhiệm việc bật/tắt Module theo từng Site (mặc định **tắt**, Site nào cần thì bật).
3. `tbl_Role_Function` và `tbl_User_Role` được denormalize thêm `SiteId` (copy từ
   `tbl_Roles`) để lọc nhanh và làm nền cho Row-Level Security.

Không có server nào chạy sẵn để test các script này — đã rà logic kỹ và đối chiếu đúng
tên bảng/cột/index trong `Base/Gateway/Gateway.Api/SQL/BaseSQL.sql`, nhưng **bắt buộc chạy thử trên
một bản backup/staging trước khi chạy lên production**, đặc biệt các bước có `DROP
TABLE`/`DROP COLUMN`.

## Cách chạy

**[2026-09-01] Không còn cần chạy gì cả cho DB mới.** 9 file migration (`00`-`08`) từng nằm ở
đây đã bị **xoá** — toàn bộ 3 thay đổi cốt lõi ở mục "Bối cảnh" phía trên (SiteId trên
`tbl_Users`, `tbl_Site_Module`, SiteId denormalize trên `tbl_Role_Function`/`tbl_User_Role`,
kể cả fix "RoleId unique theo (SiteId, RoleId)" ở mục bên dưới) giờ đã nằm sẵn trong
`Base/Gateway/Gateway.Api/SQL/BaseSQL.sql` — chỉ cần chạy file đó (kèm `Base/Gateway/Gateway.Api/SQL/seed_admin_account.sql`
nếu cần tài khoản test) là có ngay schema ở đúng trạng thái cuối cùng, không cần tiến hoá dần
qua từng bước ALTER nữa.

9 file cũ chỉ có giá trị khi tiến hoá 1 DB `BaseRepo` **đã tồn tại từ trước** theo schema chưa
multi-tenant (chưa có `SiteId` trên `tbl_Users`, chưa có `tbl_Site_Module`...) — môi trường hiện
tại luôn tạo DB mới từ đầu nên không còn tình huống đó. Nội dung giải thích **lý do** của từng
thay đổi (bug tìm thấy, cách sửa, rủi ro khi deploy) vẫn giữ nguyên bên dưới vì vẫn mô tả đúng
hành vi C# hiện tại — chỉ riêng phần "chạy file nào theo thứ tự nào" không còn áp dụng được.

## Không nằm trong các script này

Đây là phần schema/dữ liệu. Theo mục 06 của tài liệu thiết kế, các việc sau nằm ở tầng
ứng dụng, **không** có trong SQL. Phần code C# đã viết (xem "Code C# đã viết" bên dưới)
phủ gần hết danh sách này — phần còn thiếu được đánh dấu rõ.

- ✅ **Đã viết**: `AuthorizationInputDTO` thêm `SiteId` (nullable) — bắt buộc chỉ khi hệ
  thống có nhiều hơn 1 Site đang active, tự suy ra khi chỉ có đúng 1 Site.
- ✅ **Đã viết**: `Gateway.Domain.Entities.User` thêm `SiteId`; `AuthorizationRepository.Login()`
  tra `tbl_Users`/`tbl_User_Role`/`tbl_Role_Function` có lọc `SiteId`; `Logout()` cũng lọc
  thêm `SiteId` (đọc từ claim `site_id` trong token) vì `UserId` không còn unique toàn cục.
- ✅ **Đã viết**: Đổi định dạng Redis cache key sang theo `AutoId` — token có thêm 2 claim
  mới `auto_id` (khoá cache) và `site_id` (Site của user); `AuthorizationMiddleware` (dùng
  chung cho cả Gateway lẫn Site service) đọc theo đúng 2 claim này khi cache miss.
- ✅ **Đã viết**: Feature CQRS `SiteModuleController`/`ISiteModuleRepository` cho
  `tbl_Site_Module` — xem chi tiết ở mục "Code C# đã viết" bên dưới.
- ✅ **Đã fix** (đợt này): `CreateUserRole` đổi điều kiện tiền đề từ "có UserSite active"
  sang "Role.SiteId = User.SiteId" — xem mục "Fix: các Repository RoleId-aware còn thiếu
  sót" bên dưới.
- ⚠️ **Chưa làm**: Row-Level Security (RLS) vẫn đang **tắt** — chưa có `SECURITY POLICY`/`SESSION_CONTEXT`
  nào được bật trên 4 bảng liên quan. File script bật RLS (`06_enable_row_level_security.sql`,
  tuỳ chọn) đã bị xoá cùng đợt dọn `00`-`08` — nếu sau này muốn bật, cần viết lại từ đầu (kèm
  wire `EXEC sp_set_session_context @key = N'SiteId', @value = @siteId` vào nơi mở connection ở
  `CommonLibs/BaseDbContext`), không có sẵn để chạy ngay.
- ✅ **Đã viết** (đợt này): Test cách ly 2 Site (UserId/RoleId trùng tên nhau giữa 2 Site,
  xác nhận không có API nào trả chéo dữ liệu) — xem mục "Test cách ly 2 Site" bên dưới, và
  checklist đã thêm vào `.claude/rules/06-testing.md`.
- ✅ **Đã fix** (xem mục "Fix: bỏ bypass Admin toàn hệ thống" ngay bên dưới): rủi ro
  `AuthorizationMiddleware` bỏ qua `RequireFunction` cho bất kỳ Role tên "Admin" nào, không
  phân biệt Site nào cấp Role đó — Admin của Site A trước đây vô tình bỏ qua được kiểm tra
  quyền trên tài nguyên của Site B.

## Fix: bỏ bypass "Admin toàn hệ thống"

**Vấn đề**: trước bản fix này, `AuthorizationMiddleware` cho bất kỳ user nào giữ Role tên
literal "Admin" bỏ qua toàn bộ kiểm tra `[RequireFunction(...)]`, không phân biệt Role đó
do Site nào cấp. Hợp lý ở thiết kế gốc (chỉ có đúng 1 Role "Admin" duy nhất, thật sự toàn hệ
thống), nhưng sau khi mỗi Site tự có Role "Admin" riêng (multi-tenant, Role được quản lý
động theo từng Site), việc so khớp literal tên Role không còn phân biệt được "Admin của Site
nào" — Admin của Site A vô tình bỏ qua được `RequireFunction` trên tài nguyên của Site B.
Cùng gốc rễ này còn xuất hiện thêm 1 chỗ nữa: `BaseController.IsSiteAdmin(siteId)` có nhánh
`user.IsInRole("Admin")` trả `true` ngay cả khi `siteId` truyền vào không phải Site của user
— dùng ở `SiteController.UpdateSite`/`DeleteSite`, nghĩa là Admin của Site A trước đây có
thể sửa/xoá row `tbl_Site` của Site B.

**Hướng đã chọn (theo yêu cầu người dùng)**: bỏ hẳn khái niệm "Admin toàn hệ thống bypass"
— không còn bất kỳ đường tắt nào bỏ qua `RequireFunction`. Role "Admin" của mỗi Site giờ là
1 Role bình thường như mọi Role khác, phải được cấp Function tường minh qua
`tbl_Role_Function`.

**Đã sửa:**

| File | Thay đổi |
|---|---|
| `CommonLibs/Middlewares/AuthorizationMiddleware.cs` | Bỏ hẳn nhánh `isGlobalAdmin` — `RequireFunction` giờ luôn đòi hỏi đúng `FunctionId` nằm trong `allowedFunctionIds`, không có ngoại lệ theo tên Role |
| `CommonLibs/Controllers/BaseController.cs` | `IsSiteAdmin(siteId)` bỏ nhánh `user.IsInRole("Admin")` — chỉ còn tin theo claim `admin_site` đã được `Login()` gán đúng theo SiteId của user |
| `Gateway/Program.cs`, `Site/Program.cs`, `AdministrativeUnit/Program.cs` | Xoá `AdminPolicy` (`RequireRole("Admin")`) — policy này chưa được gắn vào controller/action nào (dead code), xoá để không ai vô tình nối lại vào 1 endpoint sau này và tái tạo đúng lỗi vừa fix. Không dùng `[Authorize(Roles = "Admin")]` ở bất kỳ đâu vì lý do tương tự |
| `CommonLibs/Utils/RequireFunctionAttribute.cs`, comment trong `AuthorizationRepository.cs` | Cập nhật doc comment cho khớp hành vi mới |

**Hệ quả cần biết khi deploy lên 1 DB đã có dữ liệu**: ngay khi bỏ bypass, Role "Admin" của MỌI
Site sẽ mất hết quyền đã hoạt động trước đó nếu chưa có dòng tường minh trong
`tbl_Role_Function`. Script `07_backfill_admin_role_functions.sql` (đã xoá cùng đợt dọn `00`-`08`)
từng cấp lại đúng 1 lần toàn bộ Function đang active cho mọi Role tên "Admin" khi deploy đợt
code này lên DB cũ — không còn áp dụng cho DB mới tạo từ `BaseSQL.sql` (chưa có Role "Admin"
nào để backfill). Cấp quyền cho tài khoản test chuẩn dùng
`CommonLibs/BaseSQL/seed_admin_account.sql`; cấp cho Role/Site khác dùng API
`AssignRolePermissions` có sẵn.

**Sau khi deploy ổn định**, quyền cụ thể của từng Role "Admin" (theo từng Site) nên được rà
lại và thu hẹp dần qua chính API `AssignRolePermissions` có sẵn
(`Site.Application/Features/Permission`) nếu muốn quản trị theo nguyên tắc least-privilege
chặt hơn thay vì để mọi Admin luôn có mọi Function — việc này thuộc vận hành, không nằm
trong script backfill.

## Fix: RoleId vẫn unique toàn cục

**Phát hiện khi viết test cách ly 2 Site** (kịch bản bắt buộc phải có: RoleId trùng tên
giữa 2 Site — xem mục "Test cách ly 2 Site" bên dưới): `tbl_Roles.RoleId` vẫn còn nguyên
`UNIQUE INDEX` toàn cục (`IX_Roles_RoleId`) khai báo từ `BaseSQL.sql` gốc — không migration
nào trong 01-07 từng đụng tới index này. Nghĩa là trước migration `08`, 2 Site **không thể**
có Role trùng `RoleId` ở tầng DB — tiền đề cốt lõi của thiết kế multi-tenant ("mỗi Site tự
quản lý Role riêng, độc lập, có thể trùng tên với Site khác") **chưa từng thực sự đúng**, và
mọi đoạn code C# lọc `SiteId` trên các bảng liên quan `RoleId` (viết ở migration 05-07) chưa
bao giờ được test dưới điều kiện "va chạm" thật.

**Đã sửa**: file `08_fix_roleid_unique_scope.sql` (đã xoá cùng đợt dọn `00`-`08`) từng bỏ
`UNIQUE INDEX` cũ trên riêng `RoleId`, tạo `IX_Roles_SiteId_RoleId` unique theo
`(SiteId, RoleId)` trên 1 DB đã tồn tại. Với DB mới, `Base/Gateway/Gateway.Api/SQL/BaseSQL.sql` đã khai
báo sẵn `CONSTRAINT [UQ_tbl_Roles_SiteId_RoleId] UNIQUE ([SiteId], [RoleId])` ngay từ đầu —
không còn tình trạng "RoleId unique toàn cục" cần sửa nữa.

## Fix: các Repository RoleId-aware còn thiếu sót

Sau khi RoleId được phép trùng giữa 2 Site (migration `08`), rà lại toàn bộ Repository từng
đọc/ghi theo `RoleId` phát hiện thêm nhiều lỗ hổng — phần lớn là các đoạn resolve "Site của
Role" chỉ bằng `RoleId` một mình (không kèm `SiteId`), vốn an toàn khi RoleId còn unique toàn
cục nhưng trở thành **ambiguous** (mập mờ, không xác định) ngay khi RoleId được phép trùng:
`QueryFirstOrDefaultAsync` không có `ORDER BY` sẽ trả về một trong hai dòng của 2 Site một
cách không xác định.

**Đã sửa (đợt này):**

| File | Vấn đề | Cách sửa |
|---|---|---|
| `Site.Infrastructure/Repositories/ModuleRepository.cs` | Toàn bộ CRUD vẫn tham chiếu cột `tbl_Modules.SiteId` đã bị xoá từ `04_globalize_module_catalog.sql` — crash mọi request | Bỏ hoàn toàn `SiteId` khỏi Module (entity/DTO/repository) — Module đúng là danh mục toàn hệ thống |
| `Gateway.Infrastructure/Repositories/UserRepository.cs` | Toàn bộ CRUD không lọc `SiteId` — đọc/ghi/export/import chéo Site tự do; `ImportUsers`' MERGE `ON T.UserId = S.UserId` (thiếu `SiteId`) có thể ghi đè User của Site khác trùng UserId | Toàn bộ action lọc `SiteId`, MERGE `ON` thêm `SiteId`, Controller ép `SiteId = CurrentSiteId` |
| `Site.Infrastructure/Repositories/RoleRepository.cs`, `RoleFunctionRepository.cs`, `PermissionRepository.cs`, `UserRoleRepository.cs` | CRUD thiếu lọc `SiteId` trên nhiều thao tác (Create/Update/Delete/Export/Import/GetAll); 2 câu `INSERT` (`RoleFunctionRepository.CreateRoleFunction`, `PermissionRepository.AssignRolePermissions`) thiếu cột `SiteId` dù `NOT NULL` — crash | Thêm `SiteId` vào toàn bộ Input/Filter/Delete DTO liên quan, Controller ép `SiteId = CurrentSiteId`, mọi câu SQL lọc thêm `SiteId`, 2 `INSERT` thêm cột `SiteId` |
| `UserRoleRepository.CreateUserRole`, `RoleFunctionRepository.CreateRoleFunction` | Resolve "Site của Role" bằng `Select SiteId From tbl_Roles Where RoleId = @RoleId` — **không** kèm `SiteId` của người gọi → ambiguous khi RoleId trùng giữa 2 Site, có thể chặn nhầm request hợp lệ của chính Site người gọi (roleSiteId "trúng" ngẫu nhiên dòng của Site kia) | Đổi câu SELECT lọc thẳng `Where RoleId = @RoleId And SiteId = @SiteId` (SiteId của người gọi) — vừa xác nhận Role tồn tại vừa xác nhận thuộc đúng Site trong 1 bước, loại bỏ hoàn toàn ambiguity |
| `UserRoleRepository.GetUserRoleById`, `DeleteUserRole` | Cùng lỗi ambiguous ở trên, và `GetUserRoleById` (`Site/Controllers/UserRoleController.GetUserRole`), `DeleteUserRoleInputDTO` **hoàn toàn thiếu tham số `SiteId`** — không có cách nào truyền Site của người gọi xuống | Thêm `SiteId` xuyên suốt: `DeleteUserRoleInputDTO`, `GetUserRoleByIdQuery`/Handler, `IUserRoleRepository.GetUserRoleById` (thêm tham số `siteId`), Controller ép `CurrentSiteId`, Repository lọc thẳng theo `(RoleId, SiteId)` |
| `RoleRepository`, `RoleFunctionRepository`, `PermissionRepository` — `InvalidateUsersCacheByRole(roleId)` | Không lọc `SiteId` khi tìm User đang giữ Role để xoá cache Redis — sau khi Update/Delete/Import 1 Role, vô tình xoá luôn cache của User ở Site KHÁC nếu Role của Site đó tình cờ trùng `RoleId` (không phải rò rỉ dữ liệu, nhưng là hiệu ứng phụ chéo Site không mong muốn — buộc User Site khác load lại quyền không cần thiết) | Thêm tham số `siteId`, lọc thêm `ur.SiteId = @SiteId` ở cả 3 file — mọi call site đã có sẵn `SiteId` trong scope (`input.SiteId` hoặc tham số `siteId` của method) |
| Cache Redis (8 vị trí, 5 file: `PermissionRepository`, `RoleRepository`, `RoleFunctionRepository`, `FunctionRepository`, `UserRoleRepository`) | Khoá cache Redis dùng định dạng cũ theo `UserId` — sai định dạng so với `auth:user-permissions:{AutoId}` mà `Login()`/`AuthorizationMiddleware` dùng, khiến việc invalidate không có tác dụng (cache cũ vẫn còn tới khi hết TTL) | Đổi toàn bộ sang resolve `AutoId` (qua Join `tbl_Users` theo đúng `(UserId, SiteId)`) rồi mới build cache key |
| `PermissionController.GetPermissionTree` | Route nhận `{siteId}` tự do từ client, không có `[RequireFunction]` — bất kỳ user đã đăng nhập nào đổi `siteId` trên URL là xem được cây quyền/Role-Function của Site khác | Controller bỏ qua `siteId` trên route, luôn dùng `CurrentSiteId` |
| `RoleController.CreateRole`/`UpdateRole` | Client tự truyền `SiteId` tuỳ ý trong body — có thể tạo/sửa Role ở Site khác | Controller ép `data.SiteId = CurrentSiteId` |
| `Gateway.Infrastructure/Repositories/AuthorizationRepository.RenewToken()` | Câu `UPDATE tbl_Users Set RefreshToken = ... Where UserId = @UserId` thiếu `SiteId` — có thể cập nhật nhầm nhiều dòng cùng `UserId` ở các Site khác nhau | Thêm `And SiteId = @SiteId` |

**⚠️ Chưa kiểm chứng bằng compile**: môi trường viết code này không có .NET SDK
(`dotnet` không tồn tại), nên **toàn bộ thay đổi C# ở đợt này (bao gồm cả các mục ở trên và
migration `08`) mới chỉ được rà thủ công** (đối chiếu chữ ký interface/implementation, kiểu
dữ liệu, tên cột) — **chưa build/chạy thử**. Bắt buộc `dotnet build` toàn bộ solution và chạy
qua `test_two_site_isolation.sh` (xem mục ngay bên dưới) trên môi trường có SQL Server +
Redis thật trước khi deploy.

## Test cách ly 2 Site

Thư mục `tests/` (cùng cấp README này) chứa bộ test cách ly 2 Site theo đúng yêu cầu ở mục
"Không nằm trong các script này" phía trên — quy ước manual/HTTP-based, không phải xUnit/NUnit
(repo không có test project, xem `.claude/rules/06-testing.md`):

| File | Vai trò |
|---|---|
| `tests/seed_two_site_isolation.sql` | Tạo fixture: 2 Site test (`ISOTEST_A`, `ISOTEST_B`), 1 User trùng `UserId` (`isotest_user`) và 1 Role trùng `RoleId` (`ISOTEST_ROLE`) độc lập ở mỗi Site, Function cấp lệch giữa 2 Site (`ToggleSiteModule` chỉ Site A có), 1 dòng `tbl_Site_Module` chỉ bật cho Site A. "Mượn" ciphertext cột `Password` của tài khoản `admin` (test chuẩn của repo) vì `EncryptionUtils.EncryptData` mã hoá 2 chiều bằng khoá ứng dụng cố định, không thể tạo ciphertext hợp lệ bằng T-SQL thuần |
| `tests/cleanup_two_site_isolation.sql` | Xoá toàn bộ fixture trên (tiền tố `ISOTEST_`/`isotest_`), thứ tự con→cha, an toàn chạy nhiều lần |
| `tests/test_two_site_isolation.sh` | Script bash (`curl` + `jq`) gọi thẳng API qua Gateway (và cổng nội bộ Site service cho 2 action không có route YARP) — xác nhận: token của cùng `UserId` ở 2 Site có `auto_id`/`site_id` khác nhau; `GetRolesByUser`/`GetRole`/`GetAllRoleFunctions`/`GetAllUserRoles`/`GetAllUsers` không trả chéo dữ liệu dù `UserId`/`RoleId` trùng chuỗi; `PermissionController.GetPermissionTree` bỏ qua `siteId` trên route; `CreateUserRole` chặn gán Role không thuộc Site người gọi; `[RequireFunction]` cách ly đúng theo Site cấp quyền (không "ăn theo" chỉ vì Role trùng tên) |

Checklist chạy đầy đủ đã thêm vào mục 3 của `.claude/rules/06-testing.md`.

## Code C# đã viết

### 1. Login/Logout SiteId-aware

| File | Thay đổi |
|---|---|
| `Gateway.Domain/DTOs/AuthorizationDTO.cs` | `AuthorizationInputDTO` thêm `SiteId` (nullable) |
| `Gateway.Domain/Entities/User.cs` | Thêm property `SiteId` + `SetSiteId()`, cả 2 constructor (tham số + `DataRow`) đều đọc/ghi `SiteId` |
| `Gateway.Infrastructure/Repositories/AuthorizationRepository.cs` | `Login()`: thêm `ResolveSiteId()` (tự suy ra Site khi hệ thống chỉ có 1 Site, bắt buộc client truyền khi có nhiều hơn 1); tra `tbl_Users`/`tbl_User_Role`/`tbl_Role_Function` đều lọc thêm `SiteId`; token thêm 2 claim `site_id`, `auto_id`; Redis cache key đổi sang `AutoId`. `Logout()`: đọc thêm claim `site_id`, lọc `tbl_Users` theo cả `UserId` lẫn `SiteId` |
| `CommonLibs/Middlewares/AuthorizationMiddleware.cs` | Đọc claim `auto_id`/`site_id` bắt buộc phải có trong token; cache key đổi theo `auto_id`; câu SQL fallback khi cache miss lọc thêm `SiteId`; forward thêm header `X-Site-Id` xuống service con (middleware này dùng chung cho cả Gateway lẫn Site service - xem `Site/Program.cs`) |
| `CommonLibs/Controllers/BaseController.cs` | Thêm property `CurrentSiteId` (đọc claim `site_id` của user hiện tại) — dùng cho các action tự thao tác trên đúng Site của người gọi |

**Điểm quan trọng cần biết khi deploy**: sau khi đổi cấu trúc token (thêm claim bắt buộc
`auto_id`/`site_id`), token cũ (phát hành trước khi đổi) sẽ bị `AuthorizationMiddleware`
từ chối ("Token payload is not valid.") vì thiếu 2 claim mới — user đang đăng nhập cần
đăng nhập lại sau khi deploy đợt này.

### 2. SiteModuleController (tbl_Site_Module)

| File | Vai trò |
|---|---|
| `Site.Domain/Entities/SiteModule.cs` | Entity, theo đúng convention `BaseEntity` + constructor `DataRow` như `Module`/`RoleFunction` |
| `Site.Domain/DTOs/SiteModuleDTO.cs` | `ProvisionSiteModuleInputDTO`, `ToggleSiteModuleInputDTO` (có `Enable: bool` — set thẳng trạng thái đích, không "lật cờ", để idempotent), `SiteModuleOutputDTO` |
| `Site.Application/Interfaces/ISiteModuleRepository.cs` | `ProvisionSiteModule`, `GetSiteModulesBySite`, `ToggleSiteModule` |
| `Site.Application/Features/SiteModule/{ProvisionSiteModule,GetSiteModulesBySite,ToggleSiteModule}/` | Command/Query + Handler theo skill `create-application-layer` |
| `Site.Application/Mappings/SiteMapping.cs` | Thêm `CreateMap` cho `SiteModule` |
| `Site.Infrastructure/Repositories/SiteModuleRepository.cs` | Implement `ISiteModuleRepository` bằng Dapper, theo đúng pattern `ModuleRepository`/`RoleFunctionRepository` |
| `Site.Infrastructure/DependencyInjection.cs` | Đăng ký `ISiteModuleRepository` |
| `Site/Controllers/SiteModuleController.cs` | 4 action (xem bảng route bên dưới) |
| `Gateway/appsettings.json` | Thêm đúng 2 route YARP (`site-module-my-route`, `site-module-toggle-route`) — KHÔNG dùng `{**catch-all}` như các entity khác |

**Route và ai gọi được:**

| Action | Route | Qua Gateway? | Quyền |
|---|---|---|---|
| `ProvisionSiteModule` | `POST /api/site-module/provision` | ❌ Không có route YARP — chỉ gọi thẳng cổng nội bộ Site service | `[RequireFunction("ProvisionSiteModule")]`, đội vận hành |
| `GetSiteModulesBySite` | `GET /api/site-module/site/{siteId}` | ❌ Không có route YARP | `UserOrAdminPolicy`, đội vận hành (SiteId truyền tự do) |
| `GetMySiteModules` | `GET /api/site-module/my` | ✅ Có route YARP | `UserOrAdminPolicy`, SiteId luôn lấy từ claim `site_id` của người gọi |
| `ToggleSiteModule` | `PUT /api/site-module/toggle` | ✅ Có route YARP | `[RequireFunction("ToggleSiteModule")]`, SiteId luôn lấy từ claim `site_id` — Site Admin không thể set SiteId tuỳ ý qua body |

**Lưu ý thiết kế khác với mô tả gốc trong tài liệu (mục 07)**: tài liệu thiết kế gốc chỉ nói
2 endpoint "provisioning (tạo dòng, xem theo Site)" là nội bộ. Khi viết code thực tế phát
hiện nếu chỉ có đúng 2 endpoint đó thì Site Admin không có cách nào (qua Gateway) xem được
Module nào Site mình đã được cấp để mà bật/tắt — nên đã thêm route công khai
`GetMySiteModules`, dùng lại đúng logic `GetSiteModulesBySite` nhưng ép `SiteId` lấy từ
token thay vì tham số client, nên không phát sinh rủi ro đọc chéo Site. `ToggleSiteModule`
chỉ `UPDATE`, không bao giờ tự `INSERT` — trả `404` nếu Module chưa được đội vận hành cấp,
giữ đúng nguyên tắc "Site Admin không tự bật được Module chưa mua".

`ToggleSiteModule` không invalidate Redis permission cache (`auth:user-permissions:{autoId}`)
— cache đó chỉ lưu Role/Function, không liên quan trạng thái bật/tắt Module.

## Lưu ý / điểm mở

- **[2026-09-01]** `Base/Gateway/Gateway.Api/SQL/BaseSQL.sql` (đường dẫn kể từ 2026-09-02 — trước đó
  là `CommonLibs/BaseSQL/BaseSQL.sql`, xem Progress Log trong `claude.md`) đã được viết lại để có đầy đủ
  `PRIMARY KEY`/`UNIQUE`/`FOREIGN KEY` đặt tên tường minh (không còn chỉ dùng `INDEX` trần
  như file `00`-`08` cũ từng dùng) — áp dụng cho DB **tạo mới hoàn toàn**. `BaseSQL.sql`
  hiện tại đã phản ánh đúng trạng thái *sau khi* áp dụng hết mọi thay đổi mô tả trong README
  này (đã có `tbl_Users.SiteId`, `tbl_Site_Module`, `tbl_Roles` unique theo `(SiteId, RoleId)`...)
  — xem mục "Cách chạy" phía trên.
- `tbl_Roles.SiteId` khai báo `VARCHAR(50)` (khác `tbl_Site.SiteId` là `VARCHAR(20)`) — lệch
  kiểu có sẵn từ trước khi có multi-tenant, giữ nguyên trong `BaseSQL.sql` hiện tại (xem
  comment tại chỗ trong file đó). Hai cột `SiteId` trên `tbl_Role_Function`/`tbl_User_Role`
  dùng theo đúng kiểu nguồn copy (`VARCHAR(50)`) để tránh implicit convert khi so sánh — cân
  nhắc chuẩn hoá về `VARCHAR(20)` trong một đợt dọn dẹp riêng nếu muốn.
