# Roadmap — BaseRepo

> **Cập nhật:** 2026-11-16 (N4, N6 ✅) · **Phiên bản mới nhất:** v0.0.0 · **Lịch sử:** [changelog.md](changelog.md)
> **Ký hiệu:** ✅ xong · 🟡 làm một phần · ⬜ chưa làm · 🔑 cần credential/hạ tầng ngoài · 🏗️ subsystem lớn
> **Quy tắc cập nhật file này:** [.claude/rules/roadmap-sync.md](../rules/roadmap-sync.md)

Mục lục: [1. Tiến độ](#1-tổng-quan-tiến-độ) · [2. Việc còn lại](#2-việc-còn-lại) · [3. Ưu tiên](#3-ưu-tiên-tiếp-theo) · [4. Thêm tính năng](#4-cách-thêm-tính-năng) · [5. Đã xong](#5-đã-xong)

> **Lưu ý khi khởi tạo:** mọi tính năng ở mục 5 được làm **trước khi có workflow này** (nguồn: Progress Log trong
> `claude.md` ở gốc repo), nên chưa có `06-handover.md`. Chúng đã build xanh nhưng **phần lớn chưa test runtime
> end-to-end**: máy dev hiện tại chưa có SQL Server/Redis. Mỗi việc verify còn tồn đọng được tách thành một dòng
> 🔑 ở mục 2.3, và module tương ứng giữ 🟡 cho tới khi verify xong.

---

## 1. Tổng quan tiến độ

| # | Module | Trạng thái | Còn lại |
|---|---|---|---|
| 1 | Gateway: Auth, User, định tuyến YARP | 🟡 | M7 tài liệu URL mới · 🔑 M10 verify YARP · 🔑 M12 verify auth |
| 2 | Site: RBAC + multi-tenant | 🟡 | M9 RequireFunction cho Site · 🔑 M11 test cách ly 2 Site |
| 3 | AdministrativeUnit (Province/Ward) | 🟡 | M5 template import · 🔑 M13 chạy SQL + seed + test |
| 4 | Booking: Quản lý thuê sân (SportsBooking) | 🟡 | Phase 1: 🏗️ M18 Customer, M2 Booking + chống trùng lịch, M3 RecurringGroup, M4 API Lễ tân/Admin, 🔑 M14 SQL + seed · Phase 2/3: 🔑 M17, 🏗️ M19, 🏗️ M20 |
| 5 | CommonLibs / hạ tầng chung | 🟡 | M1 dựng xUnit · M8 vá NU1903 · 🔑 M15 verify runtime |
| 6 | SystemManagement (WinForms) | 🟡 | M6 màn hình SQL · 🔑 M16 verify net10 · ⬜ N3-1, N3-2 (spec đã duyệt, chưa code), N3-3..N3-9 (phác thảo) |
| 7 | BloodDonation: Đăng ký hiến máu | ⬜ | 🏗️ M21 khởi tạo domain (chờ BA) |

## 2. Việc còn lại

### 2.1 Làm được ngay (module có sẵn)

| Mã | Module | Việc | Phụ thuộc | Trạng thái |
|---|---|---|---|---|
| M1 | 5 | Dựng test project xUnit `{Service}.Tests` (theo `convention-testing.md`), bắt đầu với 1 service; cần duyệt thêm package | — | ⬜ |
| M2 | 4 | Entity + API `Booking` (BookingStatus, PaymentStatus đánh dấu thủ công) + **logic chống trùng lịch**, nghiệp vụ trọng tâm của Phase 1 | M18 | ⬜ |
| M3 | 4 | `RecurringGroup` (đặt sân định kỳ): sinh nhiều `Booking`, áp chống trùng cho từng bản ghi | M2 | ⬜ |
| M4 | 4 | API cho Lễ tân/Admin: huỷ, sửa `Booking`/`Customer`, đánh dấu thu tiền mặt. **FE nằm ở repo khác**, repo này chỉ làm API | M2, M18 | ⬜ |
| M5 | 3 | Template import mẫu `Import_Provinces_mau.xlsx`, `Import_Wards_mau.xlsx` | — | ⬜ |
| M6 | 6 | Màn hình `UC_ConfigSQL` (đọc SQL từ `ModuleItem.FolderPath`), xem spec `docs/superpowers/specs/2026-09-02-sql-file-relocation-and-system-management-sql-tool-design.md`. **Được thực hiện trong N3-2** (thuộc N3) | — | ⬜ |
| M7 | 1 | Cập nhật `docs/quan_li_thue_san/phase_1/FSR.md`, `Diagrams.md`, bảng route trong `multi-tenant/README.md` theo URL scheme mới (`/api/{service}/...`) | — | ⬜ |
| M8 | 5 | Vá cảnh báo bảo mật NU1903: `AutoMapper 13.0.1`, `System.Security.Cryptography.Xml 8.0.2`; cần duyệt việc bump package | — | ⬜ |
| M9 | 2 | Thiết kế `RequireFunction` cho `UpdateSite`/`DeleteSite` (hiện chỉ có `IsSiteAdmin`) | — | ⬜ |

### 2.2 Tính năng mới (ngoài plan gốc)

| Mã | Việc | Ghi chú | Trạng thái |
|---|---|---|---|
| N3 | Module 6: ConfigSQL — công cụ quản trị SQL Server (nhiều kết nối đồng thời không lưu, test kết nối và thực thi trực tiếp trên nhiều đích, xem đối tượng + sinh script, backup/copy sang DB khác, gợi ý index, chẩn đoán treo/kill session/clear cache). Bao gồm M6. Chia 9 task N3-1..N3-9 (spec ở `.claude/docs/tasks/N3-*/02-coding-spec.md`: N3-1 khung+kết nối → N3-2 chạy file `.sql` (M6) → N3-3 đối tượng → N3-4 backup/copy (chỉ schema / chỉ dữ liệu / cả hai) → N3-5 index → N3-6 chẩn đoán + kill/cache → N3-7 restore → N3-8 thư viện script chẩn đoán kể cả log hệ thống → N3-9 so sánh cấu trúc giữa 2 kết nối và migration tự động: thêm cột, sửa kiểu theo nguồn, tự xử lý phụ thuộc; không so sánh dữ liệu), xem `.claude/docs/tasks/N3-configsql-quan-tri-sql-server/01-requirement.md` | Yêu cầu đã chốt 2026-11-15; spec N3-1, N3-2 chi tiết đã duyệt 2026-11-15 (chưa code), N3-3..N3-9 là bản phác thảo cần BA hoàn thiện; cần 🔑 SQL Server 2019+ để verify runtime | ⬜ |

### 2.3 Cần credential / hạ tầng ngoài 🔑

| Mã | Việc | Cần gì | Trạng thái |
|---|---|---|---|
| M10 | Verify URL scheme YARP mới cho cả 3 cluster, theo checklist 5 bước trong Progress Log. **Quan trọng nhất:** `ProvisionSiteModule`/`GetSiteModulesBySite` KHÔNG được gọi được qua Gateway | Máy có SQL Server + Redis, chạy đủ 4 service | ⬜ |
| M11 | Chạy `test_two_site_isolation.sh` (seed → test → cleanup), xem `06-testing.md` mục 3 | SQL Server + Redis, Gateway + Site đang chạy | ⬜ |
| M12 | Verify `renew-token` (happy path + 2 edge case), fix `UserOrAdminPolicy`, fix 502 header Unicode (`GET /api/site/permission/...` với Role "Ký thuật viên") | SQL Server + Redis | ⬜ |
| M13 | AdministrativeUnit: chạy `BaseAdministrativeUnit.sql` + seed 01–05, xác nhận login DB, test API đọc anonymous + API ghi | SQL Server (DB `AdministrativeUnit`) | ⬜ |
| M14 | Booking: chạy `BaseBooking.sql`, seed `Function`/`Role_Function` cho các action mutating hiện có | SQL Server | ⬜ |
| M15 | Verify runtime .NET 10 cho 4 service + log SQL (`executing/completed/failed`, tham số bị che) + `SensitiveHeaderMasker` (không lộ token/cookie khi có exception) | SQL Server + Redis | ⬜ |
| M16 | Verify `SystemManagement` trên net10: màn hình ConfigEnv (`Cypher.*`/`EncryptionLibrary`), publish `SingleFile`/`SelfContained` | Máy Windows có DevExpress | ⬜ |
| M17 | Phase 2 Booking: tích hợp cổng thanh toán online VNPay/MoMo/ZaloPay | Tài khoản merchant/sandbox + credential, chờ BA | ⬜ |

### 2.4 Subsystem lớn 🏗️ (mỗi cái một sprint riêng)

| Mã | Việc | Ghi chú | Trạng thái |
|---|---|---|---|
| M18 | Booking Phase 1: `Customer` đăng ký/đăng nhập (Email/SĐT + mật khẩu), `CustomerAuthController` với **JWT scheme riêng** (không đi qua `AuthorizationMiddleware`/RBAC), `CustomerPolicy` theo claim `CustomerId` | Đụng tới auth; spec có sẵn ở `docs/superpowers/specs/2026-09-01-booking-phase1-design.md`, cần plan | ⬜ |
| M19 | Phase 2 Booking: POS bán hàng hoá | Chờ BA | ⬜ |
| M20 | Phase 3 Booking: thống kê / Power BI | Chờ BA | ⬜ |
| M21 | BloodDonation: khởi tạo domain "Đăng ký hiến máu" (service mới theo khuôn 5-layer, `DomainGroup` = `BloodDonation/`) | Chưa có spec, chờ BA. Có thể có dữ liệu cá nhân/y tế, cần xem lại cấu hình chặn secret | ⬜ |

## 3. Ưu tiên tiếp theo

_Thứ tự gợi ý, chỉ chứa việc CHƯA xong. Hướng đã chốt: **ổn định nền trước, Booking sau**._

1. M1: Dựng xUnit (làm được trên máy hiện tại, là nền cho mọi task sau)
2. M10, M11, M12, M15: Verify YARP, cách ly 2 Site, auth, runtime .NET 10 (khi có máy SQL Server/Redis)
3. M8: Vá NU1903
4. M14: Chạy SQL + seed Booking
5. M18: Customer (🏗️, sprint riêng)
6. M2 → M3 → M4: Booking + chống trùng lịch, RecurringGroup, API Lễ tân/Admin
7. M13, M5: AdministrativeUnit
8. M7, M9: Tài liệu URL mới, RequireFunction cho Site
9. N3 (N3-1 → N3-2 → ...; gồm M6), M16: SystemManagement (N3 cần SQL Server 2019+ để verify runtime)
10. M17, M19, M20, M21: Booking Phase 2/3 và BloodDonation (chờ BA)

## 4. Cách thêm tính năng

Dùng mã `N` kế tiếp (**hiện tại: N7**). Thêm dòng vào mục 2.2, tăng số `N` ở đây, đề xuất vị trí ở mục 3.

## 5. Đã xong

_Theo module. Mỗi module có anchor `xong-<số>`. Mỗi dòng kèm link tới `.claude/docs/tasks/<ID>-<slug>/06-handover.md`.
Các mục dưới đây làm trước khi có workflow, không có handover; chi tiết xem Progress Log trong `claude.md`.
Mức xác nhận: code hoàn chỉnh + build xanh, chưa test runtime trừ khi ghi khác._

### xong-1 — Gateway: Auth, User, định tuyến YARP
- Login/Logout, Role + Function nạp vào JWT claim và cache Redis (`auth:user-permissions:{userId}`).
- `POST /api/authorization/renew-token` (cookie set ở Controller, không ở Repository). Verify: M12.
- User API: Create/Update/Delete (mềm/cứng)/GetById/GetAll/Import (mã hoá mật khẩu)/Export/ChangePassword.
- `AuthorizationMiddleware` dùng chung ở CommonLibs (defense in depth), header `X-User-Role`/`X-User-Functions` encode ASCII.
- YARP: URL scheme theo tên service (`/api/site|administrativeunit|booking/...`), gộp route từ 16 xuống 10. Verify: M10.
- Swagger gộp qua Gateway (`/site-swagger`, `/administrativeunit-swagger`, `/booking-swagger`).

### xong-2 — Site: RBAC + multi-tenant
- CRUD + Import/Export: Site, Module, Role, Screen, Function, RoleFunction, UserRole, SiteModule.
- Permission API: cây Module → Screen → Function theo Site + đồng bộ quyền Role trong 1 transaction.
- `[RequireFunction]` thay `AdminPolicy` trên 31 action mutating; `IsSiteAdmin` cho UpdateSite/DeleteSite.
- Invalidate cache Redis chủ động khi sửa User_Role/Role_Function/Role/Function.
- Multi-tenant: user thuộc đúng 1 Site (`tbl_Users.SiteId`, đã bỏ `UserSite`), `RoleId` trùng được giữa các Site. Verify cách ly: M11.

### xong-3 — AdministrativeUnit
- Service riêng (port 8002, DB riêng): CRUD + Import/Export Province/Ward, `GET /ward/by-province/{code}`; API đọc anonymous, API ghi cần auth. Verify: M13.

### xong-4 — Booking (SportsBooking)
- Phase 1 Foundation: service 5-layer (port 8003), CRUD `FieldType`/`Field`/`PricingRule`, Site-scoped (đã vá IDOR cross-Site), `BaseBooking.sql`. Vận hành: M14.

### xong-5 — CommonLibs / hạ tầng chung
- Kiến trúc 5-layer đồng nhất cho 4 service, thư mục gom theo `DomainGroup` (`Base/`, `SportsBooking/`, `BloodDonation/`).
- Nâng toàn bộ solution lên .NET 10 (bump EF Core, JwtBearer, SqlClient, YARP 2.3, Swashbuckle 10). Verify: M15.
- Tách `appsettings.Serilog.json` cho từng service.
- Log SQL có che tham số (`SqlLoggingHelper`) + `SensitiveHeaderMasker` dùng chung (vá lỗi log token thô ở `GlobalExceptionMiddleware`). Verify: M15.
- Xoá `CommonLibs/Consul/` (dead code); chuyển file SQL vào `{Service}.Api/SQL/` của service sở hữu.

### xong-6 — SystemManagement
- Nâng lên `net10.0-windows` (build xanh, fix analyzer `WFO1000`). Verify: M16.
- **N1** — ConfigEnv tab "Tùy chỉnh": thêm/sinh value (AES, GUID urn, chuỗi ngẫu nhiên, cặp RSA)/sửa/xóa biến env Machine, 12 biến hệ thống cốt lõi chỉ cho xem. Build xanh, test tự động PASS, review APPROVE, người dùng chạy tay checklist đạt 2026-11-15. Commit `73ca84a`. [06-handover.md](tasks/N1-configenv-env-tuy-chinh/06-handover.md)
- **N2** — ConfigEnv sửa biến hệ thống cốt lõi: **N2-1** mở khóa sửa 9 biến cốt lõi không phải danh sách (elevated + gõ lại tên key), backup bắt buộc vào `Backups/Env/` trước khi ghi, ghi qua registry giữ kiểu `REG_EXPAND_SZ`/`REG_SZ`, nút Khôi phục; **N2-2** trình sửa từng mục (thêm/sửa/xóa/lên/xuống, cảnh báo trùng và đường dẫn không tồn tại, giới hạn 2047 ký tự) cho `Path`/`PATHEXT`/`PSModulePath`. Build xanh, test tự động PASS (N2-1 54/54, N2-2 37/37), review APPROVE, người dùng chạy tay checklist đạt 2026-11-16. Commit `c73bdbb`, `052d90a`. [N2-1 handover](tasks/N2-1-backup-khoi-phuc-bien-loi/06-handover.md) · [N2-2 handover](tasks/N2-2-trinh-sua-danh-sach/06-handover.md)
- **N5** — ConfigEnv: chuyển 4 form (`FrmEnvTypeKeyConfirm`, `FrmEnvRestorePicker`, `FrmEnvListEditor`, `FrmCustomEnvEditor` tách ra file riêng) từ dựng bằng code sang Visual Studio Designer, không đổi hành vi; thêm quy tắc `.claude/rules/convention-winforms.md`. Build xanh, test tự động PASS (harness so cây control cũ/mới), review APPROVE, người dùng chạy tay đạt 2026-11-16 (kể cả DPI cao). [06-handover.md](tasks/N5-configenv-chuyen-form-sang-designer/06-handover.md)
- **N6** — ConfigEnv loader + chạy nền cho mọi Lưu/Xóa/Khôi phục (sửa lỗi "trông như đơ"), `Frm_Base` (cờ `IsSaving`, khóa form, chặn đóng, thanh `pb_Saving` riêng từng form) và mọi form kế thừa `Frm_Base`. Build xanh, test tự động PASS (harness 52/52), review APPROVE, người dùng chạy tay đạt 2026-11-16. Commit `8d9116e`, `6e63d53`, `ab68c24`. [06-handover.md](tasks/N6-configenv-loader-chay-nen/06-handover.md)
- **N4** — ConfigEnv tab Tùy chỉnh hai lưới **System (Machine)** và **User (HKCU)**, hiện mọi biến kể cả 6 key `BaseRepo__*` (sửa/xóa như biến thường), backup bắt buộc trước mọi sửa/xóa/khôi phục ở cả hai scope (file backup có `Scope`, `Backups/` vào `.gitignore`), Khôi phục được cả biến đã xóa. **N4-1** dịch vụ theo scope (test 46/46, review APPROVE), **N4-2** giao diện hai lưới (harness 29/29, review APPROVE). Người dùng chạy tay checklist đạt 2026-11-16. Commit `b70457a`, `27c7c19`. [N4-1 handover](tasks/N4-1-dich-vu-env-theo-scope/06-handover.md) · [N4-2 handover](tasks/N4-2-giao-dien-hai-luoi-system-user/06-handover.md)

### xong-7 — BloodDonation
- _(chưa có)_
