USE Booking
GO

-- =======================================================================
-- 1. FIELDTYPE — danh mục loại sân (bóng đá, cầu lông, pickleball...), dùng chung
-- =======================================================================
CREATE TABLE [dbo].[FieldType] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [FieldTypeId] VARCHAR(20) NOT NULL,
    [FieldTypeName] NVARCHAR(100) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_FieldType_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_FieldType] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_FieldType_FieldTypeId] UNIQUE ([FieldTypeId])
);
GO

CREATE INDEX [IX_FieldType_Status_CreatedDate_UpdatedDate]
ON [dbo].[FieldType]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 2. FIELD — sân cụ thể, thuộc 1 Site (Site service, tham chiếu qua Id, không FK vật lý
-- vì khác database) và 1 FieldType.
-- =======================================================================
CREATE TABLE [dbo].[Field] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [FieldId] VARCHAR(20) NOT NULL,
    [SiteId] VARCHAR(20) NOT NULL,
    [FieldTypeId] VARCHAR(20) NOT NULL,
    [FieldName] NVARCHAR(100) NOT NULL,
    -- OperationalStatus: 1=Active, 2=Maintenance. Tách biệt khỏi [Status] (soft-delete chung) — xem
    -- Booking.Domain.Entities.Field.FieldOperationalStatus.
    [OperationalStatus] INT NOT NULL CONSTRAINT [DF_Field_OperationalStatus] DEFAULT 1,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_Field_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_Field] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_Field_FieldId] UNIQUE ([FieldId]),
    CONSTRAINT [FK_Field_FieldType] FOREIGN KEY ([FieldTypeId]) REFERENCES [dbo].[FieldType]([FieldTypeId])
);
GO

CREATE NONCLUSTERED INDEX [IX_Field_SiteId] ON [dbo].[Field] ([SiteId]);
CREATE NONCLUSTERED INDEX [IX_Field_FieldTypeId] ON [dbo].[Field] ([FieldTypeId]);
CREATE INDEX [IX_Field_Status_CreatedDate_UpdatedDate]
ON [dbo].[Field]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 3. PRICINGRULE — giá theo khung giờ + ngày trong tuần cho 1 Field. Không có business
-- key riêng (định danh bằng AutoId thẳng) vì bản thân là 1 dòng cấu hình giá, không phải
-- thực thể nghiệp vụ độc lập được tham chiếu từ nơi khác.
-- =======================================================================
CREATE TABLE [dbo].[PricingRule] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [FieldId] VARCHAR(20) NOT NULL,
    -- 0=Sunday .. 6=Saturday (khớp System.DayOfWeek của .NET)
    [DayOfWeek] INT NOT NULL,
    [StartTime] TIME NOT NULL,
    [EndTime] TIME NOT NULL,
    [PricePerHour] DECIMAL(12,2) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_PricingRule_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_PricingRule] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [FK_PricingRule_Field] FOREIGN KEY ([FieldId]) REFERENCES [dbo].[Field]([FieldId])
);
GO

CREATE NONCLUSTERED INDEX [IX_PricingRule_FieldId] ON [dbo].[PricingRule] ([FieldId]);
CREATE INDEX [IX_PricingRule_Status_CreatedDate_UpdatedDate]
ON [dbo].[PricingRule]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO
