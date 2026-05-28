CREATE TABLE tbl_Users (
	AutoId BIGINT IDENTITY(1,1) PRIMARY KEY
	,UserId VARCHAR(20) NOT NULL UNIQUE
	,[Password] VARCHAR(255) NOT NULL
	,UserName NVARCHAR(255) NOT NULL
	,[Priority] INT NOT NULL DEFAULT 1
	,RefreshToken VARCHAR(MAX) NULL
	,[Signature] VARBINARY(MAX) NULL
	,Email VARCHAR(255) NULL
	,QuickCode VARCHAR(20) NULL
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
CREATE UNIQUE INDEX IX_Users_UserId ON tbl_Users(UserId);
CREATE INDEX IX_Users_Email ON tbl_Users(Email);
CREATE INDEX IX_Users_QuickCode ON tbl_Users(QuickCode);
CREATE INDEX IX_Users_Status_CreatedDate_UpdatedDate ON tbl_Users( Status, CreatedDate DESC, UpdatedDate DESC );

CREATE TABLE tbl_Modules (
	AutoId BIGINT IDENTITY(1,1) PRIMARY KEY
	,ModuleId varchar(50) UNIQUE NOT NULL
	,ModuleName nvarchar(MAX) NOT NULL
	,SiteId varchar(20) NOT NULL
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
CREATE UNIQUE INDEX IX_Modules_ModuleId ON tbl_Modules(ModuleId)
CREATE INDEX IX_Modules_SiteId ON tbl_Modules(SiteId);
CREATE INDEX IX_Modules_Status_CreatedDate_UpdatedDate ON tbl_Modules(Status, CreatedDate DESC, UpdatedDate DESC);

CREATE TABLE tbl_User_Module (
	AutoId BIGINT IDENTITY(1,1) PRIMARY KEY
	,ModuleId varchar(50) NOT NULL
	,UserId VARCHAR(20) NOT NULL
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
CREATE UNIQUE INDEX IX_tbl_User_Module_UserId_ModuleId ON tbl_User_Module(UserId, ModuleId);
CREATE INDEX IX_tbl_User_Module_ModuleId ON tbl_User_Module(ModuleId);
CREATE INDEX IX_tbl_User_Module_UserId ON tbl_User_Module(UserId);
CREATE INDEX IX_tbl_User_Module_Status_CreatedDate_UpdatedDate ON tbl_User_Module(Status, CreatedDate DESC, UpdatedDate DESC);

CREATE TABLE tbl_Site (
	AutoId BIGINT IDENTITY(1,1) PRIMARY KEY
	,SiteId varchar(20) UNIQUE NOT NULL
	,SiteName nvarchar(255) NOT NULL
	,SiteLogo varbinary(max) null
	,[Address] nvarchar(MAX) null
	,Email varchar(255) null
	,Hotline varchar(20) null
	,TaxCode varchar(50) null
	,Representative nvarchar(max) null
	,RepresentativeIdNumber varchar(12) null
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
CREATE UNIQUE INDEX IX_tbl_Site_SiteId ON tbl_Site(SiteId);
CREATE INDEX IX_tbl_Site_Status_CreatedDate_UpdatedDate ON tbl_Site(Status, CreatedDate DESC, UpdatedDate DESC);