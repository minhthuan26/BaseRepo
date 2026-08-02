/*
  Dọn bảng staging sau khi đã transform xong và kiểm tra kết quả ở 04_transform_to_target_tables.sql
  ổn (đúng 34 Province, 3321 Ward). Chỉ chạy file này SAU KHI đã xác nhận dữ liệu đích đúng.
*/

USE AdministrativeUnit;
GO

IF OBJECT_ID('staging.wards', 'U') IS NOT NULL DROP TABLE staging.wards;
IF OBJECT_ID('staging.provinces', 'U') IS NOT NULL DROP TABLE staging.provinces;
IF OBJECT_ID('staging.administrative_units', 'U') IS NOT NULL DROP TABLE staging.administrative_units;
IF OBJECT_ID('staging.administrative_regions', 'U') IS NOT NULL DROP TABLE staging.administrative_regions;
GO

DROP SCHEMA IF EXISTS staging;
GO
