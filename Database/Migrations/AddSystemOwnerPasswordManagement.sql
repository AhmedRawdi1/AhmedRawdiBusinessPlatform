USE [ARBP];
GO

SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.SystemUsers', N'IsSystemOwner') IS NULL
    ALTER TABLE dbo.SystemUsers ADD IsSystemOwner bit NOT NULL CONSTRAINT DF_SystemUsers_IsSystemOwner DEFAULT (0);

IF COL_LENGTH(N'dbo.SystemUsers', N'MustChangePassword') IS NULL
    ALTER TABLE dbo.SystemUsers ADD MustChangePassword bit NOT NULL CONSTRAINT DF_SystemUsers_MustChangePassword DEFAULT (0);

IF COL_LENGTH(N'dbo.SystemUsers', N'SecurityStamp') IS NULL
    ALTER TABLE dbo.SystemUsers ADD SecurityStamp uniqueidentifier NOT NULL CONSTRAINT DF_SystemUsers_SecurityStamp DEFAULT (NEWID());

IF OBJECT_ID(N'dbo.SystemSecurityAudit', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemSecurityAudit
    (
        ID bigint NOT NULL CONSTRAINT PK_SystemSecurityAudit PRIMARY KEY,
        EventType varchar(80) NOT NULL,
        TargetUserID bigint NULL,
        PerformedByUserID bigint NULL,
        EventDateUtc datetime2(0) NOT NULL CONSTRAINT DF_SystemSecurityAudit_EventDateUtc DEFAULT (SYSUTCDATETIME()),
        Details nvarchar(1000) NULL,
        CONSTRAINT FK_SystemSecurityAudit_TargetUser FOREIGN KEY (TargetUserID) REFERENCES dbo.SystemUsers(ID),
        CONSTRAINT FK_SystemSecurityAudit_PerformedBy FOREIGN KEY (PerformedByUserID) REFERENCES dbo.SystemUsers(ID)
    );
    CREATE SEQUENCE dbo.Seq_SystemSecurityAuditID AS bigint START WITH 1 INCREMENT BY 1;
END;

EXEC sys.sp_executesql N'
UPDATE dbo.SystemUsers
SET IsSystemOwner = CASE WHEN ID = 1 AND Code = N''Admin'' THEN 1 ELSE 0 END
WHERE IsSystemOwner <> CASE WHEN ID = 1 AND Code = N''Admin'' THEN 1 ELSE 0 END;';

DECLARE @OwnerExists bit = 0;
EXEC sys.sp_executesql N'SELECT @Found=CONVERT(bit, CASE WHEN EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID=1 AND Code=N''Admin'' AND IsSystemOwner=1) THEN 1 ELSE 0 END);', N'@Found bit OUTPUT', @Found=@OwnerExists OUTPUT;
IF @OwnerExists = 0
    THROW 50200, 'The expected Admin system owner account was not found.', 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.SystemUsers') AND name=N'UX_SystemUsers_OneSystemOwner')
    EXEC(N'CREATE UNIQUE INDEX UX_SystemUsers_OneSystemOwner ON dbo.SystemUsers(IsSystemOwner) WHERE IsSystemOwner = 1;');

COMMIT TRANSACTION;
GO
