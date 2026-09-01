USE AdministrativeUnit
GO

-- Sau khi chạy xong file này, seed dữ liệu 34 Province + 3.321 Ward bằng
-- seed_administrative_unit.sql (cùng thư mục này, xem README.md cùng thư mục).

-- =======================================================================
-- 1. TẠO BẢNG TỈNH / THÀNH PHỐ (CẤP 1)
-- =======================================================================
CREATE TABLE [dbo].[Province] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [Code] VARCHAR(20) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Type] NVARCHAR(50) NULL,
    [NameWithType] NVARCHAR(150) NULL,
    [Slug] VARCHAR(150) NULL,

    -- Các trường hệ thống
    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_Province_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    -- Giống mọi bảng khác trong Base/Gateway/Gateway.Api/SQL/BaseSQL.sql: AutoId là PRIMARY KEY (clustered),
    -- Code (business key) chỉ là UNIQUE INDEX (non-clustered) chứ không phải PK.
    CONSTRAINT [PK_Province] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_Province_Code] UNIQUE ([Code])
);
GO

CREATE INDEX [IX_Province_Status_CreatedDate_UpdatedDate]
ON [dbo].[Province]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 2. TẠO BẢNG PHƯỜNG / XÃ / ĐẶC KHU (CẤP 2 — thuộc thẳng Tỉnh)
-- =======================================================================
-- Từ 01/07/2025, Việt Nam sáp nhập đơn vị hành chính theo Nghị quyết 202/2025/QH15: còn 34
-- tỉnh/thành, bỏ hẳn cấp Quận/Huyện — chỉ còn 2 cấp Tỉnh -> Xã/Phường trực tiếp. Vì vậy schema
-- này không còn bảng District; Ward tham chiếu thẳng Province qua ProvinceCode.
CREATE TABLE [dbo].[Ward] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [Code] VARCHAR(20) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Type] NVARCHAR(50) NULL,
    [NameWithType] NVARCHAR(150) NULL,
    [ProvinceCode] VARCHAR(20) NOT NULL,
    [Slug] VARCHAR(150) NULL,

    -- Các trường hệ thống
    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_Ward_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    -- Giống mọi bảng khác trong Base/Gateway/Gateway.Api/SQL/BaseSQL.sql: AutoId là PRIMARY KEY (clustered),
    -- Code (business key) chỉ là UNIQUE INDEX (non-clustered) chứ không phải PK.
    CONSTRAINT [PK_Ward] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_Ward_Code] UNIQUE ([Code])
);
GO

CREATE NONCLUSTERED INDEX [IX_Ward_ProvinceCode] ON [dbo].[Ward] ([ProvinceCode]);
CREATE INDEX [IX_Ward_Status_CreatedDate_UpdatedDate]
ON [dbo].[Ward]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO
