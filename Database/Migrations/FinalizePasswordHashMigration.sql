USE [ARBP];
GO

SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.SystemUsers', N'UserPass') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE UserPass IS NOT NULL)
        THROW 50100, 'Password migration is incomplete; reversible password values still exist.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM sys.sql_expression_dependencies
        WHERE referenced_id = OBJECT_ID(N'dbo.SystemUsers')
          AND referenced_minor_id = COLUMNPROPERTY(OBJECT_ID(N'dbo.SystemUsers'), N'UserPass', 'ColumnId')
    )
        THROW 50101, 'A database object still depends on SystemUsers.UserPass.', 1;

    ALTER TABLE dbo.SystemUsers DROP COLUMN UserPass;
END;
GO
