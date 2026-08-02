/*
  Transform dữ liệu từ bảng staging (nguyên bản từ nguồn mở) sang đúng schema Province/Ward
  của bạn.

  Mapping:
    - Province.Code         <- staging.provinces.code
    - Province.Name         <- staging.provinces.name              (VD 'Hà Nội')
    - Province.Type         <- staging.administrative_units.short_name (VD 'Thành phố' / 'Tỉnh')
    - Province.NameWithType <- staging.provinces.full_name          (VD 'Thành phố Hà Nội')
    - Province.Slug         <- staging.provinces.code_name          (VD 'ha_noi')

    - Ward.Code         <- staging.wards.code
    - Ward.Name         <- staging.wards.name                      (VD 'Ba Đình')
    - Ward.Type         <- staging.administrative_units.short_name  (VD 'Phường' / 'Xã' / 'Đặc khu')
    - Ward.NameWithType <- staging.wards.full_name                  (VD 'Phường Ba Đình')
    - Ward.ProvinceCode <- staging.wards.province_code
    - Ward.Slug         <- staging.wards.code_name                  (VD 'ba_dinh')

  Status dùng default DF_Province_Status/DF_Ward_Status = 1 (Active) sẵn có trong schema, không
  cần chỉ định trong INSERT. Các cột hệ thống khác (CreatedBy/UpdatedBy/Device*) để NULL — app
  tầng trên tự điền theo user/thiết bị thực hiện thao tác, migration này không phải là 1 "user".

  ⚠️ Chạy 1 lần trên bảng Province/Ward đang RỖNG. Nếu chạy lại lần 2 mà chưa xoá dữ liệu cũ sẽ
  bị lỗi trùng khoá chính (Code). Muốn chạy lại từ đầu, tự chạy (cân nhắc kỹ vì đây là DELETE):
      TRUNCATE TABLE [dbo].[Ward];
      TRUNCATE TABLE [dbo].[Province];
  trước khi chạy lại file này.
*/

USE AdministrativeUnit;
GO

INSERT INTO [dbo].[Province] ([Code], [Name], [Type], [NameWithType], [Slug], [CreatedDate])
SELECT
    p.code,
    p.name,
    au.short_name,
    p.full_name,
    p.code_name,
    SYSDATETIME()
FROM staging.provinces p
JOIN staging.administrative_units au ON au.id = p.administrative_unit_id;
GO

INSERT INTO [dbo].[Ward] ([Code], [Name], [Type], [NameWithType], [ProvinceCode], [Slug], [CreatedDate])
SELECT
    w.code,
    w.name,
    au.short_name,
    w.full_name,
    w.province_code,
    w.code_name,
    SYSDATETIME()
FROM staging.wards w
JOIN staging.administrative_units au ON au.id = w.administrative_unit_id;
GO

-- Kiểm tra nhanh số dòng vừa insert (kỳ vọng: 34 Province, 3321 Ward)
SELECT COUNT(*) AS ProvinceCount FROM [dbo].[Province];
SELECT COUNT(*) AS WardCount FROM [dbo].[Ward];
GO
