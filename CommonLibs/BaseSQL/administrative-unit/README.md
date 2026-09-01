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
đây là script tạo schema **từ đầu** cho DB `AdministrativeUnit` hoàn toàn mới.

## Nguồn dữ liệu

[thanglequoc/vietnamese-provinces-database](https://github.com/thanglequoc/vietnamese-provinces-database),
tag [`v3.0.2`](https://github.com/thanglequoc/vietnamese-provinces-database/releases/tag/v3.0.2)
("Breaking Change: Nghị Quyết 202/2025/QH15", công bố 2025-07-28) — dữ liệu build sẵn theo đúng
[toàn văn 34 nghị quyết của UBTVQH về sắp xếp đơn vị hành chính cấp xã](https://xaydungchinhsach.chinhphu.vn/toan-van-34-nghi-quyet-cua-ubtvqh-ve-sap-xep-cac-don-vi-hanh-chinh-cap-xa-119250616215143373.htm).
File gốc dùng: `sqlserver/ImportData_vn_units.sql` — tải nguyên văn ngày 2026-08-02, không chỉnh sửa
dữ liệu (xem đầu file `seed_administrative_unit.sql`).

Số liệu: **34 Province**, **3.321 Ward**.

## Cách chạy

```
1. BaseAdministrativeUnit.sql        -- tạo schema Province/Ward
2. seed_administrative_unit.sql      -- seed 34 Province + 3.321 Ward trong 1 file
```

`seed_administrative_unit.sql` tự nạp dữ liệu gốc vào **local temp table** (tiền tố `#`) rồi
transform sang `Province`/`Ward` trong cùng 1 file — tự huỷ temp table khi phiên kết nối kết
thúc, không cần file dọn dẹp riêng. An toàn chạy lại nhiều lần (`INSERT` vào `Province`/`Ward`
tự bỏ qua nếu bảng đích đã có dữ liệu).

**[2026-09-01]** Trước đây quy trình này chạy qua 5 file `01`-`05` riêng (schema 3 cấp cũ còn
bảng `District` → ALTER bỏ `District` → tạo bảng `staging` → nạp dữ liệu → transform → dọn
`staging`). Đã gộp lại thành 2 file ở trên và **xoá 5 file cũ** vì môi trường hiện tại luôn tạo
DB mới từ đầu (drop-and-recreate khi cần đổi schema), không còn DB nào ở schema 3 cấp cũ cần
tiến hoá dần nữa.

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

- `seed_administrative_unit.sql` tự bỏ qua nếu `Province`/`Ward` đã có dữ liệu (an toàn chạy lại
  nhiều lần) — muốn seed lại từ đầu với dữ liệu mới, tự `TRUNCATE TABLE [dbo].[Ward];
  TRUNCATE TABLE [dbo].[Province];` trước khi chạy lại file.
- `Slug` lấy nguyên `code_name` từ nguồn (dùng gạch dưới `_`, VD `ba_dinh`), không tự đổi sang
  gạch ngang `-` — nếu FE/BE cần format slug khác (VD `ba-dinh`), xử lý transform riêng, không sửa
  trực tiếp trong các script này.
- Nếu sau này có nghị quyết điều chỉnh thêm (sáp nhập/tách tiếp), cập nhật bằng cách lặp lại quy
  trình này với bản release mới hơn của nguồn dữ liệu, không tự sửa tay từng dòng.
