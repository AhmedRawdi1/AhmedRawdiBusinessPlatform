CREATE OR ALTER PROCEDURE [dbo].[usp_Add_SystemUserPermissions]
    @UserID BIGINT,
    @GroupID BIGINT = NULL,
    @PermissionsJson NVARCHAR(MAX),
    @RegUserID BIGINT = NULL,
    @HasError BIT OUTPUT,
    @ErrorDesc NVARCHAR(2048) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @HasError = 0;
    SET @ErrorDesc = NULL;

    BEGIN TRY
        IF ISNULL(@UserID, 0) = 0
            THROW 50001, 'User ID is required.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WITH (NOLOCK) WHERE ID = @UserID)
            THROW 50002, 'The selected system user does not exist.', 1;

        IF @PermissionsJson IS NULL OR LTRIM(RTRIM(@PermissionsJson)) = N'' OR ISJSON(@PermissionsJson) = 0
            THROW 50003, 'A valid JSON array of form permissions is required.', 1;

        BEGIN TRANSACTION;

        SELECT
            FormID,
            CAST(ISNULL(CanSave, 0) AS BIT)   AS CanSave,
            CAST(ISNULL(CanUpdate, 0) AS BIT) AS CanUpdate,
            CAST(ISNULL(CanDelete, 0) AS BIT) AS CanDelete,
            CAST(ISNULL(CanSearch, 0) AS BIT) AS CanSearch,
            CAST(ISNULL(CanPrint, 0) AS BIT)  AS CanPrint
        INTO #ParsedUserPermissions
        FROM OPENJSON(@PermissionsJson)
        WITH (
            FormID    BIGINT   '$.FormID',
            CanSave   BIT      '$.CanSave',
            CanUpdate BIT      '$.CanUpdate',
            CanDelete BIT      '$.CanDelete',
            CanSearch BIT      '$.CanSearch',
            CanPrint  BIT      '$.CanPrint'
        );

        MERGE INTO dbo.SystemFormsPermissions WITH (UPDLOCK, HOLDLOCK) AS Target
        USING #ParsedUserPermissions AS Source
        ON (Target.UserID = @UserID 
            AND Target.FormID = Source.FormID 
            AND Target.CancelledDate IS NULL)
        WHEN MATCHED THEN
            UPDATE SET
                CanSave   = Source.CanSave,
                CanUpdate = Source.CanUpdate,
                CanDelete = Source.CanDelete,
                CanSearch = Source.CanSearch,
                CanPrint  = Source.CanPrint,
                RegUserID = ISNULL(@RegUserID, Target.RegUserID),
                RegDate   = GETDATE()
        WHEN NOT MATCHED THEN
            INSERT (
                FormID,
                GroupID,
                UserID,
                CanSave,
                CanUpdate,
                CanDelete,
                CanSearch,
                CanPrint,
                RegUserID,
                RegDate
            )
            VALUES (
                Source.FormID,
                @GroupID,
                @UserID,
                Source.CanSave,
                Source.CanUpdate,
                Source.CanDelete,
                Source.CanSearch,
                Source.CanPrint,
                ISNULL(@RegUserID, 1),
                GETDATE()
            );

        DROP TABLE #ParsedUserPermissions;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        IF OBJECT_ID('tempdb..#ParsedUserPermissions') IS NOT NULL
            DROP TABLE #ParsedUserPermissions;

        SET @HasError = 1;
        SET @ErrorDesc = ERROR_MESSAGE();

        THROW;
    END CATCH;
END;
