USE [ARBP];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Add_SystemUserPermissions
    @UserID bigint,
    @GroupID bigint = NULL,
    @PermissionsJson nvarchar(max),
    @RegUserID bigint = NULL,
    @HasError bit OUTPUT,
    @ErrorDesc nvarchar(2048) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT @HasError = 0, @ErrorDesc = NULL;

    BEGIN TRY
        SELECT @GroupID = COALESCE(@GroupID, GroupID) FROM dbo.SystemUsers WHERE ID = @UserID AND CancellationDate IS NULL;
        IF @GroupID IS NULL THROW 50002, 'The selected system user does not exist.', 1;
        IF ISJSON(@PermissionsJson) <> 1 THROW 50003, 'A valid JSON array of form permissions is required.', 1;

        CREATE TABLE #Permissions
        (
            FormID bigint NOT NULL PRIMARY KEY,
            CanView bit NOT NULL,
            CanSave bit NOT NULL,
            CanUpdate bit NOT NULL,
            CanDelete bit NOT NULL,
            CanSearch bit NOT NULL,
            CanPrint bit NOT NULL
        );

        INSERT #Permissions (FormID, CanView, CanSave, CanUpdate, CanDelete, CanSearch, CanPrint)
        SELECT FormID, ISNULL(CanView, 0), ISNULL(CanSave, 0), ISNULL(CanUpdate, 0), ISNULL(CanDelete, 0), ISNULL(CanSearch, 0), ISNULL(CanPrint, 0)
        FROM OPENJSON(@PermissionsJson)
        WITH (FormID bigint '$.FormID', CanView bit '$.CanView', CanSave bit '$.CanSave', CanUpdate bit '$.CanUpdate', CanDelete bit '$.CanDelete', CanSearch bit '$.CanSearch', CanPrint bit '$.CanPrint');

        IF EXISTS (SELECT 1 FROM #Permissions p LEFT JOIN dbo.SystemForms f ON f.FormID = p.FormID WHERE f.FormID IS NULL)
            THROW 50004, 'One or more system forms do not exist.', 1;

        BEGIN TRANSACTION;

        UPDATE target WITH (UPDLOCK, SERIALIZABLE)
        SET GroupID = @GroupID, CanView = source.CanView, CanSave = source.CanSave, CanUpdate = source.CanUpdate,
            CanDelete = source.CanDelete, CanSearch = source.CanSearch, CanPrint = source.CanPrint,
            RegUserID = COALESCE(@RegUserID, target.RegUserID), RegDate = GETDATE()
        FROM dbo.SystemFormsPermissions target
        INNER JOIN #Permissions source ON source.FormID = target.FormID
        WHERE target.UserID = @UserID AND target.CancelledDate IS NULL;

        INSERT dbo.SystemFormsPermissions (FormID, GroupID, UserID, CanView, CanSave, CanUpdate, CanDelete, CanSearch, CanPrint, RegUserID)
        SELECT source.FormID, @GroupID, @UserID, source.CanView, source.CanSave, source.CanUpdate, source.CanDelete, source.CanSearch, source.CanPrint, COALESCE(@RegUserID, 1)
        FROM #Permissions source
        WHERE NOT EXISTS
        (
            SELECT 1 FROM dbo.SystemFormsPermissions target WITH (UPDLOCK, SERIALIZABLE)
            WHERE target.FormID = source.FormID AND target.UserID = @UserID AND target.CancelledDate IS NULL
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        SELECT @HasError = 1, @ErrorDesc = ERROR_MESSAGE();
        THROW;
    END CATCH;
END;
GO
