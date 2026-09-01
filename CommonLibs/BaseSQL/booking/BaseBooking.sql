USE Booking
GO

-- =======================================================================
-- LƯU Ý VẬN HÀNH: 9 FunctionId dùng bởi [RequireFunction("...")] trên các action ghi của Booking
-- (CreateFieldType, UpdateFieldType, DeleteFieldType, CreateField, UpdateField, DeleteField,
-- CreatePricingRule, UpdatePricingRule, DeletePricingRule) không tự có sẵn — theo thiết kế RBAC
-- của repo, không có "Admin bypass" toàn cục, nên phải tạo các Function này qua Function API rồi
-- gán cho Role tương ứng qua Role_Function trước khi tài khoản staff gọi được các endpoint ghi này.
-- =======================================================================

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

-- =======================================================================
-- LƯU Ý VẬN HÀNH (phần Phase 1 còn lại — Customer/Booking/RecurringGroup): thêm 3 FunctionId
-- mới dùng bởi [RequireFunction("...")] khi Application/Presentation layer được viết:
-- ConfirmBooking, CreateWalkInBooking, CancelBooking (gán cho cả Role Admin và Lễ tân) — cùng
-- quy ước với 9 FunctionId ở đầu file, tạo qua Function API + gán Role_Function trước khi dùng.
-- =======================================================================

-- =======================================================================
-- 4. CUSTOMER — khách đặt sân, tách biệt hoàn toàn khỏi Gateway User (tbl_Users). Đăng nhập
-- bằng Phone HOẶC Email (spec mục 4) nên cả 2 cột đều UNIQUE để tra cứu không mập mờ.
-- PasswordHash NULL = khách guest thuần, chưa từng đăng ký tài khoản (spec mục 3.2).
-- =======================================================================
CREATE TABLE [dbo].[Customer] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [CustomerId] VARCHAR(20) NOT NULL,
    [FullName] NVARCHAR(255) NOT NULL,
    [Phone] VARCHAR(20) NOT NULL,
    [Email] VARCHAR(255) NOT NULL,
    [PasswordHash] VARCHAR(255) NULL,
    [DateOfBirth] DATE NULL,
    [Gender] INT NULL,
    [Address] NVARCHAR(MAX) NULL,
    [MarketingConsent] BIT NOT NULL CONSTRAINT [DF_Customer_MarketingConsent] DEFAULT 0,
    [Source] NVARCHAR(100) NULL,
    [Note] NVARCHAR(MAX) NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_Customer_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_Customer_CustomerId] UNIQUE ([CustomerId]),
    CONSTRAINT [UQ_Customer_Phone] UNIQUE ([Phone]),
    CONSTRAINT [UQ_Customer_Email] UNIQUE ([Email])
);
GO

CREATE INDEX [IX_Customer_Status_CreatedDate_UpdatedDate]
ON [dbo].[Customer]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 5. RECURRINGGROUP — nhãn nhóm cho chuỗi Booking định kỳ, chỉ để trace/hủy cả chuỗi (spec
-- mục 3.1/3.3). CustomerId cố ý NOT NULL (khác Booking.CustomerId nullable) — spec không liệt
-- kê GuestName/GuestPhone cho entity này, đọc là chỉ Customer đã biết (không phải guest thuần)
-- mới thiết lập được lịch định kỳ; từng Booking con sinh ra từ nhóm này vẫn có thể null
-- CustomerId độc lập nếu sau này nghiệp vụ đổi ý — đây là điểm spec chưa nói rõ, ghi chú lại
-- để xác nhận khi implement Application layer.
-- =======================================================================
CREATE TABLE [dbo].[RecurringGroup] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [RecurringGroupId] VARCHAR(20) NOT NULL,
    [FieldId] VARCHAR(20) NOT NULL,
    [CustomerId] VARCHAR(20) NOT NULL,
    -- 0=Sunday .. 6=Saturday (khớp System.DayOfWeek của .NET, cùng quy ước PricingRule.DayOfWeek)
    [DayOfWeek] INT NOT NULL,
    [StartTime] TIME NOT NULL,
    [EndTime] TIME NOT NULL,
    [RangeStartDate] DATE NOT NULL,
    [RangeEndDate] DATE NOT NULL,
    -- RecurringStatus nghiệp vụ riêng (1=Active, 2=Cancelled), TÁCH BIỆT khỏi [Status] soft-delete
    -- chung — xem spec mục 3.1.
    [RecurringStatus] INT NOT NULL CONSTRAINT [DF_RecurringGroup_RecurringStatus] DEFAULT 1,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_RecurringGroup_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_RecurringGroup] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_RecurringGroup_RecurringGroupId] UNIQUE ([RecurringGroupId]),
    CONSTRAINT [FK_RecurringGroup_Field] FOREIGN KEY ([FieldId]) REFERENCES [dbo].[Field]([FieldId]),
    CONSTRAINT [FK_RecurringGroup_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer]([CustomerId])
);
GO

CREATE NONCLUSTERED INDEX [IX_RecurringGroup_FieldId] ON [dbo].[RecurringGroup]([FieldId]);
CREATE NONCLUSTERED INDEX [IX_RecurringGroup_CustomerId] ON [dbo].[RecurringGroup]([CustomerId]);
CREATE INDEX [IX_RecurringGroup_Status_CreatedDate_UpdatedDate]
ON [dbo].[RecurringGroup]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 6. BOOKING — 1 lượt đặt sân cụ thể (spec mục 3.1). CustomerId NULL nếu guest chưa đăng ký
-- (khi đó bắt buộc có GuestName/GuestPhone — ràng buộc này enforce ở Application layer bằng
-- CHECK constraint bên dưới, không chỉ dựa vào code C#, vì đây là bất biến dữ liệu quan trọng).
-- =======================================================================
CREATE TABLE [dbo].[Booking] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [BookingId] VARCHAR(20) NOT NULL,
    [FieldId] VARCHAR(20) NOT NULL,
    [CustomerId] VARCHAR(20) NULL,
    [GuestName] NVARCHAR(255) NULL,
    [GuestPhone] VARCHAR(20) NULL,
    [BookingDate] DATE NOT NULL,
    [StartTime] TIME NOT NULL,
    [EndTime] TIME NOT NULL,
    -- BookingStatus nghiệp vụ riêng (1=Pending, 2=Confirmed, 3=CheckedIn, 4=Completed,
    -- 5=Cancelled, 6=NoShow), TÁCH BIỆT khỏi [Status] soft-delete chung — xem spec mục 3.1.
    [BookingStatus] INT NOT NULL CONSTRAINT [DF_Booking_BookingStatus] DEFAULT 1,
    [TotalPrice] DECIMAL(12,2) NOT NULL,
    -- PaymentStatus: 1=Unpaid, 2=Deposited, 3=Paid — Phase 1 chỉ đánh dấu thủ công (spec mục 1/6).
    [PaymentStatus] INT NOT NULL CONSTRAINT [DF_Booking_PaymentStatus] DEFAULT 1,
    [RecurringGroupId] VARCHAR(20) NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_Booking_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_Booking] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_Booking_BookingId] UNIQUE ([BookingId]),
    CONSTRAINT [FK_Booking_Field] FOREIGN KEY ([FieldId]) REFERENCES [dbo].[Field]([FieldId]),
    CONSTRAINT [FK_Booking_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer]([CustomerId]),
    CONSTRAINT [FK_Booking_RecurringGroup] FOREIGN KEY ([RecurringGroupId]) REFERENCES [dbo].[RecurringGroup]([RecurringGroupId]),
    -- Bất biến: hoặc có CustomerId (khách đã đăng ký), hoặc có đủ GuestName+GuestPhone (guest) —
    -- không được thiếu cả 2, tránh Booking "vô danh" không liên hệ được.
    CONSTRAINT [CK_Booking_CustomerOrGuest] CHECK (
        [CustomerId] IS NOT NULL
        OR ([GuestName] IS NOT NULL AND [GuestPhone] IS NOT NULL)
    )
);
GO

CREATE NONCLUSTERED INDEX [IX_Booking_CustomerId] ON [dbo].[Booking]([CustomerId]);
CREATE NONCLUSTERED INDEX [IX_Booking_RecurringGroupId] ON [dbo].[Booking]([RecurringGroupId]);
CREATE NONCLUSTERED INDEX [IX_Booking_BookingStatus] ON [dbo].[Booking]([BookingStatus]);
-- Index quan trọng nhất bảng này: phục vụ trực tiếp truy vấn "chống trùng lịch" (spec mục 3.3,
-- gọi đây là "rule quan trọng nhất hệ thống") — luôn lọc theo FieldId + BookingDate trước khi
-- so khớp overlap StartTime/EndTime.
CREATE NONCLUSTERED INDEX [IX_Booking_FieldId_BookingDate] ON [dbo].[Booking]([FieldId], [BookingDate]);
CREATE INDEX [IX_Booking_Status_CreatedDate_UpdatedDate]
ON [dbo].[Booking]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO
