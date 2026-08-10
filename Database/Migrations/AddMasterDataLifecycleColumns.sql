SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Nationalities', 'IsActive') IS NULL
    ALTER TABLE dbo.Nationalities ADD IsActive bit NOT NULL CONSTRAINT DF_Nationalities_IsActive DEFAULT (1) WITH VALUES;
IF COL_LENGTH('dbo.Nationalities', 'RegDate') IS NULL
    ALTER TABLE dbo.Nationalities ADD RegDate datetime2(0) NOT NULL CONSTRAINT DF_Nationalities_RegDate DEFAULT (SYSUTCDATETIME()) WITH VALUES;
IF COL_LENGTH('dbo.Nationalities', 'RegUserID') IS NULL ALTER TABLE dbo.Nationalities ADD RegUserID bigint NULL;
IF COL_LENGTH('dbo.Nationalities', 'CancelDate') IS NULL ALTER TABLE dbo.Nationalities ADD CancelDate datetime2(0) NULL;
IF COL_LENGTH('dbo.Nationalities', 'CancelUserID') IS NULL ALTER TABLE dbo.Nationalities ADD CancelUserID bigint NULL;
IF COL_LENGTH('dbo.Nationalities', 'RowVersion') IS NULL ALTER TABLE dbo.Nationalities ADD RowVersion rowversion NOT NULL;

IF COL_LENGTH('dbo.Gender', 'IsActive') IS NULL
    ALTER TABLE dbo.Gender ADD IsActive bit NOT NULL CONSTRAINT DF_Gender_IsActive DEFAULT (1) WITH VALUES;
IF COL_LENGTH('dbo.Gender', 'RegDate') IS NULL
    ALTER TABLE dbo.Gender ADD RegDate datetime2(0) NOT NULL CONSTRAINT DF_Gender_RegDate DEFAULT (SYSUTCDATETIME()) WITH VALUES;
IF COL_LENGTH('dbo.Gender', 'RegUserID') IS NULL ALTER TABLE dbo.Gender ADD RegUserID bigint NULL;
IF COL_LENGTH('dbo.Gender', 'CancelDate') IS NULL ALTER TABLE dbo.Gender ADD CancelDate datetime2(0) NULL;
IF COL_LENGTH('dbo.Gender', 'CancelUserID') IS NULL ALTER TABLE dbo.Gender ADD CancelUserID bigint NULL;
IF COL_LENGTH('dbo.Gender', 'RowVersion') IS NULL ALTER TABLE dbo.Gender ADD RowVersion rowversion NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Nationalities_RegUserID')
    ALTER TABLE dbo.Nationalities WITH CHECK ADD CONSTRAINT FK_Nationalities_RegUserID FOREIGN KEY (RegUserID) REFERENCES dbo.SystemUsers(ID);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Nationalities_CancelUserID')
    ALTER TABLE dbo.Nationalities WITH CHECK ADD CONSTRAINT FK_Nationalities_CancelUserID FOREIGN KEY (CancelUserID) REFERENCES dbo.SystemUsers(ID);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Gender_RegUserID')
    ALTER TABLE dbo.Gender WITH CHECK ADD CONSTRAINT FK_Gender_RegUserID FOREIGN KEY (RegUserID) REFERENCES dbo.SystemUsers(ID);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Gender_CancelUserID')
    ALTER TABLE dbo.Gender WITH CHECK ADD CONSTRAINT FK_Gender_CancelUserID FOREIGN KEY (CancelUserID) REFERENCES dbo.SystemUsers(ID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Nationalities') AND name='IX_Nationalities_IsActive')
    CREATE INDEX IX_Nationalities_IsActive ON dbo.Nationalities(IsActive, Code);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Gender') AND name='IX_Gender_IsActive')
    CREATE INDEX IX_Gender_IsActive ON dbo.Gender(IsActive, Code);

COMMIT TRANSACTION;
