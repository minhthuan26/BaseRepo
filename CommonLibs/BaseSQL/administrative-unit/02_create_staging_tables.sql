/*
  Tạo schema + bảng staging để nạp NGUYÊN VẸN dữ liệu gốc từ nguồn mở, trước khi transform
  sang đúng schema Province/Ward của bạn (xem 04_transform_to_target_tables.sql).

  Cấu trúc bảng staging giữ đúng theo file gốc `sqlserver/CreateTables_vn_units.sql` của:
  https://github.com/thanglequoc/vietnamese-provinces-database (tag v3.0.2, 2025-07-28) — không
  đổi tên cột/kiểu dữ liệu, chỉ đặt trong schema riêng `staging` để không đụng vào bảng thật.
*/

USE AdministrativeUnit;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'staging')
    EXEC('CREATE SCHEMA staging');
GO

IF OBJECT_ID('staging.wards', 'U') IS NOT NULL DROP TABLE staging.wards;
IF OBJECT_ID('staging.provinces', 'U') IS NOT NULL DROP TABLE staging.provinces;
IF OBJECT_ID('staging.administrative_units', 'U') IS NOT NULL DROP TABLE staging.administrative_units;
IF OBJECT_ID('staging.administrative_regions', 'U') IS NOT NULL DROP TABLE staging.administrative_regions;
GO

CREATE TABLE staging.administrative_regions (
    id integer NOT NULL,
    name nvarchar(255) NOT NULL,
    name_en nvarchar(255) NOT NULL,
    code_name nvarchar(255) NULL,
    code_name_en nvarchar(255) NULL,
    CONSTRAINT PK_staging_administrative_regions PRIMARY KEY (id)
);
GO

CREATE TABLE staging.administrative_units (
    id integer NOT NULL,
    full_name nvarchar(255) NULL,
    full_name_en nvarchar(255) NULL,
    short_name nvarchar(255) NULL,
    short_name_en nvarchar(255) NULL,
    code_name nvarchar(255) NULL,
    code_name_en nvarchar(255) NULL,
    CONSTRAINT PK_staging_administrative_units PRIMARY KEY (id)
);
GO

CREATE TABLE staging.provinces (
    code nvarchar(20) NOT NULL,
    name nvarchar(255) NOT NULL,
    name_en nvarchar(255) NULL,
    full_name nvarchar(255) NOT NULL,
    full_name_en nvarchar(255) NULL,
    code_name nvarchar(255) NULL,
    administrative_unit_id integer NULL,
    CONSTRAINT PK_staging_provinces PRIMARY KEY (code)
);
GO

CREATE TABLE staging.wards (
    code nvarchar(20) NOT NULL,
    name nvarchar(255) NOT NULL,
    name_en nvarchar(255) NULL,
    full_name nvarchar(255) NULL,
    full_name_en nvarchar(255) NULL,
    code_name nvarchar(255) NULL,
    province_code nvarchar(20) NULL,
    administrative_unit_id integer NULL,
    CONSTRAINT PK_staging_wards PRIMARY KEY (code)
);
GO
