USE [ARBP];
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dbo.SystemUsers', N'PasswordHash') IS NULL
        ALTER TABLE dbo.SystemUsers ADD PasswordHash nvarchar(512) NULL;

    IF COL_LENGTH(N'dbo.SystemUsers', N'PasswordChangedAt') IS NULL
        ALTER TABLE dbo.SystemUsers ADD PasswordChangedAt datetime2(0) NULL;

    IF COL_LENGTH(N'dbo.SystemUsers', N'RowVersion') IS NULL
        ALTER TABLE dbo.SystemUsers ADD RowVersion rowversion NOT NULL;

    IF COL_LENGTH(N'dbo.SystemGroups', N'RowVersion') IS NULL
        ALTER TABLE dbo.SystemGroups ADD RowVersion rowversion NOT NULL;

    UPDATE dbo.SystemUsers
    SET Code = CONCAT(N'USER_', ID)
    WHERE NULLIF(LTRIM(RTRIM(Code)), N'') IS NULL;

    UPDATE dbo.SystemUsers
    SET EngName = Code
    WHERE NULLIF(LTRIM(RTRIM(EngName)), N'') IS NULL;

    UPDATE dbo.SystemUsers
    SET ArbName = EngName
    WHERE NULLIF(LTRIM(RTRIM(ArbName)), N'') IS NULL;

    UPDATE dbo.SystemUsers SET IsActive = 0 WHERE IsActive IS NULL;
    UPDATE dbo.SystemUsers SET RegDate = GETDATE() WHERE RegDate IS NULL;
    UPDATE dbo.SystemUsers SET PreferredLanguage = 'en-US'
    WHERE PreferredLanguage IS NULL OR PreferredLanguage NOT IN ('en-US', 'ar-SA');

    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.SystemUsers') AND name = N'UQ_SystemUserCode')
        ALTER TABLE dbo.SystemUsers DROP CONSTRAINT UQ_SystemUserCode;

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.SystemUsers') AND name = N'IX_SystemUsers_GroupID')
        DROP INDEX IX_SystemUsers_GroupID ON dbo.SystemUsers;

    ALTER TABLE dbo.SystemUsers ALTER COLUMN Code nvarchar(100) NOT NULL;
    ALTER TABLE dbo.SystemUsers ALTER COLUMN EngName nvarchar(150) NOT NULL;
    ALTER TABLE dbo.SystemUsers ALTER COLUMN ArbName nvarchar(150) NOT NULL;
    ALTER TABLE dbo.SystemUsers ALTER COLUMN IsActive bit NOT NULL;
    ALTER TABLE dbo.SystemUsers ALTER COLUMN RegDate smalldatetime NOT NULL;
    ALTER TABLE dbo.SystemUsers ALTER COLUMN PreferredLanguage varchar(25) NOT NULL;

    ALTER TABLE dbo.SystemUsers ADD CONSTRAINT UQ_SystemUserCode UNIQUE (Code);
    CREATE INDEX IX_SystemUsers_GroupID ON dbo.SystemUsers(GroupID, IsActive);

    IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.SystemUsers') AND parent_column_id = COLUMNPROPERTY(OBJECT_ID(N'dbo.SystemUsers'), N'IsActive', 'ColumnId'))
        ALTER TABLE dbo.SystemUsers ADD CONSTRAINT DF_SystemUsers_IsActive DEFAULT (1) FOR IsActive;

    IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.SystemUsers') AND parent_column_id = COLUMNPROPERTY(OBJECT_ID(N'dbo.SystemUsers'), N'RegDate', 'ColumnId'))
        ALTER TABLE dbo.SystemUsers ADD CONSTRAINT DF_SystemUsers_RegDate DEFAULT (GETDATE()) FOR RegDate;

    IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.SystemUsers') AND parent_column_id = COLUMNPROPERTY(OBJECT_ID(N'dbo.SystemUsers'), N'PreferredLanguage', 'ColumnId'))
        ALTER TABLE dbo.SystemUsers ADD CONSTRAINT DF_SystemUsers_PreferredLanguage DEFAULT ('en-US') FOR PreferredLanguage;

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.SystemUsers') AND name = N'CK_SystemUsers_PreferredLanguage')
        ALTER TABLE dbo.SystemUsers WITH CHECK ADD CONSTRAINT CK_SystemUsers_PreferredLanguage CHECK (PreferredLanguage IN ('en-US', 'ar-SA'));

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.SystemUsers') AND name = N'CK_SystemUsers_ExpiredDate')
        ALTER TABLE dbo.SystemUsers WITH CHECK ADD CONSTRAINT CK_SystemUsers_ExpiredDate CHECK (ExpiredDate IS NULL OR ExpiredDate >= RegDate);

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.SystemGroups') AND name = N'CK_SystemGroups_ExpiredDate')
        ALTER TABLE dbo.SystemGroups WITH CHECK ADD CONSTRAINT CK_SystemGroups_ExpiredDate CHECK (ExpiredDate IS NULL OR ExpiredDate >= RegDate);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.SystemUsers') AND name = N'IX_SystemUsers_ActiveDirectory')
        CREATE INDEX IX_SystemUsers_ActiveDirectory ON dbo.SystemUsers(IsActive, CancellationDate) INCLUDE (Code, EngName, ArbName, GroupID, Email);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.SystemGroups') AND name = N'IX_SystemGroups_ActiveDirectory')
        CREATE INDEX IX_SystemGroups_ActiveDirectory ON dbo.SystemGroups(IsActive, CancellationDate) INCLUDE (Code, EngName, ArbName);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE object_id = OBJECT_ID(N'dbo.Seq_SystemUserID'))
BEGIN
    DECLARE @NextUserID bigint = ISNULL((SELECT MAX(ID) FROM dbo.SystemUsers), 0) + 1;
    DECLARE @CreateUserSequenceSql nvarchar(max) = N'CREATE SEQUENCE dbo.Seq_SystemUserID AS bigint START WITH ' + CONVERT(nvarchar(30), @NextUserID) + N' INCREMENT BY 1 CACHE 25;';
    EXEC sys.sp_executesql @CreateUserSequenceSql;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE object_id = OBJECT_ID(N'dbo.Seq_SystemGroupID'))
BEGIN
    DECLARE @NextGroupID bigint = ISNULL((SELECT MAX(ID) FROM dbo.SystemGroups), 0) + 1;
    DECLARE @CreateGroupSequenceSql nvarchar(max) = N'CREATE SEQUENCE dbo.Seq_SystemGroupID AS bigint START WITH ' + CONVERT(nvarchar(30), @NextGroupID) + N' INCREMENT BY 1 CACHE 10;';
    EXEC sys.sp_executesql @CreateGroupSequenceSql;
END;
GO
