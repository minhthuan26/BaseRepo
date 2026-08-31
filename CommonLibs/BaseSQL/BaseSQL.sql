USE BaseRepo
GO

CREATE TABLE tbl_Users (
	AutoId BIGINT IDENTITY(1,1) PRIMARY KEY
	,UserId VARCHAR(20) NOT NULL
	,SiteId VARCHAR(20) NOT NULL
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
CREATE UNIQUE INDEX IX_Users_SiteId_UserId ON tbl_Users(SiteId, UserId);
CREATE INDEX IX_Users_SiteId ON tbl_Users(SiteId);
CREATE INDEX IX_Users_Email ON tbl_Users(Email);
CREATE INDEX IX_Users_QuickCode ON tbl_Users(QuickCode);
CREATE INDEX IX_Users_Status_CreatedDate_UpdatedDate ON tbl_Users( Status, CreatedDate DESC, UpdatedDate DESC );

CREATE TABLE tbl_Modules (
	AutoId BIGINT IDENTITY(1,1) PRIMARY KEY
	,ModuleId varchar(50) UNIQUE NOT NULL
	,ModuleName nvarchar(MAX) NOT NULL
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
CREATE UNIQUE INDEX IX_Modules_ModuleId ON tbl_Modules(ModuleId)
CREATE INDEX IX_Modules_Status_CreatedDate_UpdatedDate ON tbl_Modules(Status, CreatedDate DESC, UpdatedDate DESC);

CREATE TABLE tbl_Site_Module (
	AutoId BIGINT IDENTITY(1,1) PRIMARY KEY
	,SiteId VARCHAR(20) NOT NULL
	,ModuleId VARCHAR(50) NOT NULL
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
CREATE UNIQUE INDEX IX_tbl_Site_Module_SiteId_ModuleId ON tbl_Site_Module(SiteId, ModuleId);
CREATE INDEX IX_tbl_Site_Module_SiteId ON tbl_Site_Module(SiteId);
CREATE INDEX IX_tbl_Site_Module_ModuleId ON tbl_Site_Module(ModuleId);
CREATE INDEX IX_tbl_Site_Module_Status_CreatedDate_UpdatedDate ON tbl_Site_Module(Status, CreatedDate DESC, UpdatedDate DESC);

CREATE TABLE tbl_Site (
	AutoId BIGINT IDENTITY(1,1) PRIMARY KEY
	,SiteId varchar(20) UNIQUE NOT NULL
	,SiteName nvarchar(255) NOT NULL
	,SiteLogo varbinary(MAX) NULL
	,[Address] nvarchar(MAX) NULL
	,Email varchar(255) NULL
	,Hotline varchar(20) NULL
	,TaxCode varchar(50) NULL
	,Representative nvarchar(MAX) NULL
	,RepresentativeIdNumber varchar(12) NULL
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

CREATE TABLE tbl_Screens (
	AutoId bigint IDENTITY(1,1) PRIMARY KEY
	,ScreenId varchar(20) UNIQUE NOT NULL
	,ScreenName nvarchar(255) NOT NULL
	,ModuleId varchar(50) NOT NULL
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
CREATE UNIQUE INDEX IX_Screens_ScreenId ON tbl_Screens(ScreenId)
CREATE INDEX IX_Screens_ModuleId ON tbl_Screens(ModuleId);
CREATE INDEX IX_Screens_Status_CreatedDate_UpdatedDate ON tbl_Screens(Status, CreatedDate DESC, UpdatedDate DESC);

CREATE TABLE tbl_Functions (
	AutoId bigint IDENTITY(1,1) PRIMARY KEY
	,FunctionId varchar(20) UNIQUE NOT NULL
	,FunctionName nvarchar(255) NOT NULL
	,ScreenId varchar(50) NOT NULL
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
CREATE UNIQUE INDEX IX_Functions_FunctionId ON tbl_Functions(FunctionId)
CREATE INDEX IX_Functions_ScreenId ON tbl_Functions(ScreenId);
CREATE INDEX IX_Functions_Status_CreatedDate_UpdatedDate ON tbl_Functions(Status, CreatedDate DESC, UpdatedDate DESC);

CREATE TABLE tbl_Roles (
	AutoId bigint IDENTITY(1,1) PRIMARY KEY
	,RoleId varchar(20) NOT NULL
	,RoleName nvarchar(255) NOT NULL
	,SiteId varchar(50) NOT NULL
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
-- IX_Roles_ScreenId là tên cũ đặt nhầm, thực chất index trên cột SiteId
CREATE UNIQUE INDEX IX_Roles_SiteId_RoleId ON tbl_Roles(SiteId, RoleId)
CREATE INDEX IX_Roles_ScreenId ON tbl_Roles(SiteId);
CREATE INDEX IX_Roles_Status_CreatedDate_UpdatedDate ON tbl_Roles(Status, CreatedDate DESC, UpdatedDate DESC);

CREATE TABLE tbl_User_Role (
	AutoId BIGINT IDENTITY(1,1) PRIMARY KEY
	,RoleId varchar(50) NOT NULL
	,UserId VARCHAR(20) NOT NULL
	,SiteId VARCHAR(50) NOT NULL
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
CREATE UNIQUE INDEX IX_tbl_User_Role_UserId_RoleId ON tbl_User_Role(UserId, RoleId);
CREATE INDEX IX_tbl_User_Role_RoleId ON tbl_User_Role(RoleId);
CREATE INDEX IX_tbl_User_Role_UserId ON tbl_User_Role(UserId);
CREATE INDEX IX_tbl_User_Role_SiteId ON tbl_User_Role(SiteId);
CREATE INDEX IX_tbl_User_Role_Status_CreatedDate_UpdatedDate ON tbl_User_Role(Status, CreatedDate DESC, UpdatedDate DESC);

CREATE TABLE tbl_Role_Function (
	AutoId BIGINT IDENTITY(1,1) PRIMARY KEY
	,RoleId varchar(50) NOT NULL
	,FunctionId VARCHAR(20) NOT NULL
	,SiteId VARCHAR(50) NOT NULL
	,CreatedDate DATETIME2 NULL
	,UpdatedDate DATETIME2 NULL
	,CreatedBy NVARCHAR(20) NULL
	,UpdatedBy NVARCHAR(20) NULL
	,[Status] INT NOT NULL DEFAULT 1
	,DeviceName NVARCHAR(255) NULL
	,DeviceId NVARCHAR(255) NULL
)
CREATE UNIQUE INDEX IX_tbl_Role_Function_FunctionId_RoleId ON tbl_Role_Function(FunctionId, RoleId);
CREATE INDEX IX_tbl_Role_Function_RoleId ON tbl_Role_Function(RoleId);
CREATE INDEX IX_tbl_Role_Function_FunctionId ON tbl_Role_Function(FunctionId);
CREATE INDEX IX_tbl_Role_Function_SiteId ON tbl_Role_Function(SiteId);
CREATE INDEX IX_tbl_Role_Function_Status_CreatedDate_UpdatedDate ON tbl_Role_Function(Status, CreatedDate DESC, UpdatedDate DESC);