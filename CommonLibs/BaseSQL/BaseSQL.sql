USE BaseRepo
GO

-- =======================================================================
-- LƯU Ý: DDL dưới đây đã được sắp lại theo thứ tự phụ thuộc FK (bảng bị tham chiếu tạo trước) —
-- khác thứ tự bảng trong bản gốc chỉ ở việc tbl_Site được đưa lên đầu (do tbl_Users và
-- tbl_Site_Module đều FK tới nó). Không đổi tên bảng, tên cột, kiểu dữ liệu nào so với bản gốc.
-- =======================================================================

-- =======================================================================
-- 1. SITE
-- =======================================================================
CREATE TABLE [dbo].[tbl_Site] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [SiteId] VARCHAR(20) NOT NULL,
    [SiteName] NVARCHAR(255) NOT NULL,
    [SiteLogo] VARBINARY(MAX) NULL,
    [Address] NVARCHAR(MAX) NULL,
    [Email] VARCHAR(255) NULL,
    [Hotline] VARCHAR(20) NULL,
    [TaxCode] VARCHAR(50) NULL,
    [Representative] NVARCHAR(MAX) NULL,
    [RepresentativeIdNumber] VARCHAR(12) NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_tbl_Site_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_tbl_Site] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_tbl_Site_SiteId] UNIQUE ([SiteId])
);
GO

CREATE INDEX [IX_tbl_Site_Status_CreatedDate_UpdatedDate]
ON [dbo].[tbl_Site]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 2. USERS
-- =======================================================================
CREATE TABLE [dbo].[tbl_Users] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [UserId] VARCHAR(20) NOT NULL,
    [SiteId] VARCHAR(20) NOT NULL,
    [Password] VARCHAR(255) NOT NULL,
    [UserName] NVARCHAR(255) NOT NULL,
    [Priority] INT NOT NULL CONSTRAINT [DF_tbl_Users_Priority] DEFAULT 1,
    [RefreshToken] VARCHAR(MAX) NULL,
    [Signature] VARBINARY(MAX) NULL,
    [Email] VARCHAR(255) NULL,
    [QuickCode] VARCHAR(20) NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_tbl_Users_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_tbl_Users] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_tbl_Users_SiteId_UserId] UNIQUE ([SiteId], [UserId]),
    CONSTRAINT [FK_tbl_Users_Site] FOREIGN KEY ([SiteId]) REFERENCES [dbo].[tbl_Site]([SiteId])
);
GO

CREATE INDEX [IX_tbl_Users_SiteId] ON [dbo].[tbl_Users]([SiteId]);
CREATE INDEX [IX_tbl_Users_Email] ON [dbo].[tbl_Users]([Email]);
CREATE INDEX [IX_tbl_Users_QuickCode] ON [dbo].[tbl_Users]([QuickCode]);
CREATE INDEX [IX_tbl_Users_Status_CreatedDate_UpdatedDate]
ON [dbo].[tbl_Users]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 3. MODULES
-- =======================================================================
CREATE TABLE [dbo].[tbl_Modules] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [ModuleId] VARCHAR(50) NOT NULL,
    [ModuleName] NVARCHAR(MAX) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_tbl_Modules_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_tbl_Modules] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_tbl_Modules_ModuleId] UNIQUE ([ModuleId])
);
GO

CREATE INDEX [IX_tbl_Modules_Status_CreatedDate_UpdatedDate]
ON [dbo].[tbl_Modules]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 4. SITE_MODULE — module nào được bật cho site nào
-- =======================================================================
CREATE TABLE [dbo].[tbl_Site_Module] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [SiteId] VARCHAR(20) NOT NULL,
    [ModuleId] VARCHAR(50) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_tbl_Site_Module_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_tbl_Site_Module] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_tbl_Site_Module_SiteId_ModuleId] UNIQUE ([SiteId], [ModuleId]),
    CONSTRAINT [FK_tbl_Site_Module_Site] FOREIGN KEY ([SiteId]) REFERENCES [dbo].[tbl_Site]([SiteId]),
    CONSTRAINT [FK_tbl_Site_Module_Module] FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[tbl_Modules]([ModuleId])
);
GO

CREATE INDEX [IX_tbl_Site_Module_SiteId] ON [dbo].[tbl_Site_Module]([SiteId]);
CREATE INDEX [IX_tbl_Site_Module_ModuleId] ON [dbo].[tbl_Site_Module]([ModuleId]);
CREATE INDEX [IX_tbl_Site_Module_Status_CreatedDate_UpdatedDate]
ON [dbo].[tbl_Site_Module]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 5. SCREENS — thuộc 1 Module
-- =======================================================================
CREATE TABLE [dbo].[tbl_Screens] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [ScreenId] VARCHAR(20) NOT NULL,
    [ScreenName] NVARCHAR(255) NOT NULL,
    [ModuleId] VARCHAR(50) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_tbl_Screens_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_tbl_Screens] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_tbl_Screens_ScreenId] UNIQUE ([ScreenId]),
    CONSTRAINT [FK_tbl_Screens_Module] FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[tbl_Modules]([ModuleId])
);
GO

CREATE INDEX [IX_tbl_Screens_ModuleId] ON [dbo].[tbl_Screens]([ModuleId]);
CREATE INDEX [IX_tbl_Screens_Status_CreatedDate_UpdatedDate]
ON [dbo].[tbl_Screens]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 6. FUNCTIONS — thuộc 1 Screen
-- Lưu ý: cột ScreenId ở đây khai VARCHAR(50) trong khi tbl_Screens.ScreenId là VARCHAR(20)
-- (lệch sẵn từ bản gốc) — FK vẫn tạo được (SQL Server cho phép FK giữa 2 độ dài VARCHAR khác
-- nhau), giữ nguyên không đổi kiểu dữ liệu vì ngoài phạm vi yêu cầu lần này.
-- =======================================================================
CREATE TABLE [dbo].[tbl_Functions] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [FunctionId] VARCHAR(20) NOT NULL,
    [FunctionName] NVARCHAR(255) NOT NULL,
    [ScreenId] VARCHAR(50) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_tbl_Functions_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_tbl_Functions] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_tbl_Functions_FunctionId] UNIQUE ([FunctionId]),
    CONSTRAINT [FK_tbl_Functions_Screen] FOREIGN KEY ([ScreenId]) REFERENCES [dbo].[tbl_Screens]([ScreenId])
);
GO

CREATE INDEX [IX_tbl_Functions_ScreenId] ON [dbo].[tbl_Functions]([ScreenId]);
CREATE INDEX [IX_tbl_Functions_Status_CreatedDate_UpdatedDate]
ON [dbo].[tbl_Functions]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 7. ROLES — thuộc 1 Site
-- Lưu ý: cột SiteId ở đây khai VARCHAR(50) trong khi tbl_Site.SiteId là VARCHAR(20) (lệch sẵn
-- từ bản gốc) — giữ nguyên, xem ghi chú tương tự ở tbl_Functions phía trên.
-- =======================================================================
CREATE TABLE [dbo].[tbl_Roles] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [RoleId] VARCHAR(20) NOT NULL,
    [RoleName] NVARCHAR(255) NOT NULL,
    [SiteId] VARCHAR(50) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_tbl_Roles_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_tbl_Roles] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    -- RoleId cố ý KHÔNG unique 1 mình — được phép trùng giữa các Site khác nhau (multi-tenant),
    -- unique thật sự là theo cặp (SiteId, RoleId). Xem CommonLibs/BaseSQL/multi-tenant/08_fix_roleid_unique_scope.sql.
    CONSTRAINT [UQ_tbl_Roles_SiteId_RoleId] UNIQUE ([SiteId], [RoleId]),
    CONSTRAINT [FK_tbl_Roles_Site] FOREIGN KEY ([SiteId]) REFERENCES [dbo].[tbl_Site]([SiteId])
);
GO

-- IX_Roles_ScreenId là tên cũ đặt nhầm ở bản gốc (thực chất index trên cột SiteId) — đổi tên
-- đúng nghĩa khi viết lại constraint, không đổi cột/hành vi.
CREATE INDEX [IX_tbl_Roles_SiteId] ON [dbo].[tbl_Roles]([SiteId]);
CREATE INDEX [IX_tbl_Roles_Status_CreatedDate_UpdatedDate]
ON [dbo].[tbl_Roles]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 8. USER_ROLE — user nào có role nào (N-N, có SiteId sẵn trong bảng để FK composite)
-- Lưu ý: cột SiteId ở đây khai VARCHAR(50) trong khi tbl_Users.SiteId là VARCHAR(20) — giữ
-- nguyên như 2 ghi chú lệch kiểu dữ liệu phía trên.
-- =======================================================================
CREATE TABLE [dbo].[tbl_User_Role] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [RoleId] VARCHAR(50) NOT NULL,
    [UserId] VARCHAR(20) NOT NULL,
    [SiteId] VARCHAR(50) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_tbl_User_Role_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_tbl_User_Role] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_tbl_User_Role_UserId_RoleId] UNIQUE ([UserId], [RoleId]),
    CONSTRAINT [FK_tbl_User_Role_User] FOREIGN KEY ([SiteId], [UserId]) REFERENCES [dbo].[tbl_Users]([SiteId], [UserId]),
    CONSTRAINT [FK_tbl_User_Role_Role] FOREIGN KEY ([SiteId], [RoleId]) REFERENCES [dbo].[tbl_Roles]([SiteId], [RoleId])
);
GO

CREATE INDEX [IX_tbl_User_Role_RoleId] ON [dbo].[tbl_User_Role]([RoleId]);
CREATE INDEX [IX_tbl_User_Role_UserId] ON [dbo].[tbl_User_Role]([UserId]);
CREATE INDEX [IX_tbl_User_Role_SiteId] ON [dbo].[tbl_User_Role]([SiteId]);
CREATE INDEX [IX_tbl_User_Role_Status_CreatedDate_UpdatedDate]
ON [dbo].[tbl_User_Role]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO

-- =======================================================================
-- 9. ROLE_FUNCTION — role nào được gán function nào (N-N, có SiteId sẵn để FK composite tới Roles)
-- =======================================================================
CREATE TABLE [dbo].[tbl_Role_Function] (
    [AutoId] BIGINT IDENTITY(1,1) NOT NULL,
    [RoleId] VARCHAR(50) NOT NULL,
    [FunctionId] VARCHAR(20) NOT NULL,
    [SiteId] VARCHAR(50) NOT NULL,

    [CreatedDate] DATETIME2 NULL,
    [UpdatedDate] DATETIME2 NULL,
    [CreatedBy] NVARCHAR(20) NULL,
    [UpdatedBy] NVARCHAR(20) NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_tbl_Role_Function_Status] DEFAULT 1,
    [DeviceName] NVARCHAR(255) NULL,
    [DeviceId] NVARCHAR(255) NULL,

    CONSTRAINT [PK_tbl_Role_Function] PRIMARY KEY CLUSTERED ([AutoId] ASC),
    CONSTRAINT [UQ_tbl_Role_Function_FunctionId_RoleId] UNIQUE ([FunctionId], [RoleId]),
    CONSTRAINT [FK_tbl_Role_Function_Role] FOREIGN KEY ([SiteId], [RoleId]) REFERENCES [dbo].[tbl_Roles]([SiteId], [RoleId]),
    CONSTRAINT [FK_tbl_Role_Function_Function] FOREIGN KEY ([FunctionId]) REFERENCES [dbo].[tbl_Functions]([FunctionId])
);
GO

CREATE INDEX [IX_tbl_Role_Function_RoleId] ON [dbo].[tbl_Role_Function]([RoleId]);
CREATE INDEX [IX_tbl_Role_Function_FunctionId] ON [dbo].[tbl_Role_Function]([FunctionId]);
CREATE INDEX [IX_tbl_Role_Function_SiteId] ON [dbo].[tbl_Role_Function]([SiteId]);
CREATE INDEX [IX_tbl_Role_Function_Status_CreatedDate_UpdatedDate]
ON [dbo].[tbl_Role_Function]([Status], [CreatedDate] DESC, [UpdatedDate] DESC);
GO
