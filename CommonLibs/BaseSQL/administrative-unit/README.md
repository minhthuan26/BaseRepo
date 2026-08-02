# Insert đơn vị hành chính mới (sau sáp nhập 01/07/2025) vào DB AdministrativeUnit

## Bối cảnh

Từ 01/07/2025, Việt Nam sáp nhập đơn vị hành chính theo **Nghị quyết 202/2025/QH15** của Quốc hội:
63 tỉnh/thành → còn **34 tỉnh/thành**, và **bỏ hẳn cấp Quận/Huyện** — chỉ còn 2 cấp
**Tỉnh → Xã/Phường** trực tiếp.

Schema `AdministrativeUnit` gốc gửi ban đầu có 3 bảng (Province/District/Ward, 3 cấp). Vì cấp Huyện
không còn tồn tại và không có dữ liệu "mới" nào để insert vào `District`, các script ở đây:

- **Bỏ bảng `District`**, đổi `Ward.DistrictCode` → `Ward.ProvinceCode`.
- Chỉ insert dữ liệu vào `Province` và `Ward`.

`BaseAdministrativeUnit.sql` (cạnh thư mục này) đã được chuẩn hoá theo đúng cấu trúc 2 cấp này —
đây là script tạo schema **từ đầu** cho DB `AdministrativeUnit` hoàn toàn mới. `01_migrate_schema.sql`
chỉ cần dùng khi DB đã tồn tại từ trước theo schema 3 cấp cũ (đã có bảng `District`/`Ward.DistrictCode`)
và cần ALTER lại cho khớp — DB mới tạo từ `BaseAdministrativeUnit.sql` thì bỏ qua bước này.

## Nguồn dữ liệu

[thanglequoc/vietnamese-provinces-database](https://github.com/thanglequoc/vietnamese-provinces-database),
tag [`v3.0.2`](https://github.com/thanglequoc/vietnamese-provinces-database/releases/tag/v3.0.2)
("Breaking Change: Nghị Quyết 202/2025/QH15", công bố 2025-07-28) — dữ liệu build sẵn theo đúng
[toàn văn 34 nghị quyết của UBTVQH về sắp xếp đơn vị hành chính cấp xã](https://xaydungchinhsach.chinhphu.vn/toan-van-34-nghi-quyet-cua-ubtvqh-ve-sap-xep-cac-don-vi-hanh-chinh-cap-xa-119250616215143373.htm).
File gốc dùng: `sqlserver/ImportData_vn_units.sql` — tải nguyên văn ngày 2026-08-02, không chỉnh sửa
dữ liệu, chỉ đổi tên bảng đích sang schema `staging` (xem đầu file `03_seed_staging_data.sql`).

Số liệu: **34 Province**, **3.321 Ward**.

## Cách chạy (theo đúng thứ tự)

Chạy tuần tự trên SQL Server, database `AdministrativeUnit` (mỗi file dùng `sqlserver` client,
VD `sqlcmd`, SSMS, Azure Data Studio):

1. `01_migrate_schema.sql` — **chỉ chạy nếu DB đã tồn tại theo schema 3 cấp cũ** (đã có bảng
   `District`) và cần ALTER lại. Nếu tạo DB mới từ `BaseAdministrativeUnit.sql` thì bỏ qua bước
   này — schema đã đúng 2 cấp sẵn.
   ⚠️ File có DROP TABLE — đọc kỹ ghi chú đầu file trước khi chạy, đặc biệt nếu DB đã có dữ liệu thật.
2. `02_create_staging_tables.sql` — tạo schema `staging` + 4 bảng tạm đúng cấu trúc nguồn gốc.
3. `03_seed_staging_data.sql` — nạp dữ liệu gốc (nguyên văn, không chỉnh sửa) vào bảng staging.
4. `04_transform_to_target_tables.sql` — map dữ liệu từ staging sang `dbo.Province` / `dbo.Ward`,
   in ra số dòng để kiểm tra (kỳ vọng 34 / 3321).
5. `05_cleanup_staging.sql` — xoá bảng + schema staging sau khi đã xác nhận kết quả đúng.

## Mapping field

| Bảng đích | Cột | Lấy từ nguồn | Ví dụ |
|---|---|---|---|
| Province | Code | `provinces.code` | `01` |
| Province | Name | `provinces.name` | `Hà Nội` |
| Province | Type | `administrative_units.short_name` (join qua `administrative_unit_id`) | `Thành phố` / `Tỉnh` |
| Province | NameWithType | `provinces.full_name` | `Thành phố Hà Nội` |
| Province | Slug | `provinces.code_name` | `ha_noi` |
| Ward | Code | `wards.code` | `00004` |
| Ward | Name | `wards.name` | `Ba Đình` |
| Ward | Type | `administrative_units.short_name` | `Phường` / `Xã` / `Đặc khu` |
| Ward | NameWithType | `wards.full_name` | `Phường Ba Đình` |
| Ward | ProvinceCode | `wards.province_code` | `01` |
| Ward | Slug | `wards.code_name` | `ba_dinh` |

`Status` dùng default `1` (Active) sẵn có trong schema. `CreatedBy`/`UpdatedBy`/`DeviceName`/`DeviceId`
để `NULL` — migration không phải là 1 user/thiết bị thật, để tầng ứng dụng tự điền khi có thao tác
thật sau này.

## Lưu ý / điểm mở

- Script giả định `Province`/`Ward` đang **rỗng** khi chạy lần đầu. Chạy lại lần 2 mà chưa
  `TRUNCATE` sẽ lỗi trùng khoá chính `Code` (xem ghi chú trong `04_transform_to_target_tables.sql`).
- `Slug` lấy nguyên `code_name` từ nguồn (dùng gạch dưới `_`, VD `ba_dinh`), không tự đổi sang
  gạch ngang `-` — nếu FE/BE cần format slug khác (VD `ba-dinh`), xử lý transform riêng, không sửa
  trực tiếp trong các script này.
- Nếu sau này có nghị quyết điều chỉnh thêm (sáp nhập/tách tiếp), cập nhật bằng cách lặp lại quy
  trình này với bản release mới hơn của nguồn dữ liệu, không tự sửa tay từng dòng.
