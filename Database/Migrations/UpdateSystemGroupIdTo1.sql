SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE ID = 1)
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE ID = 1 AND Code = N'SAG')
       AND NOT EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE ID = 0)
        RETURN;

    THROW 50010, 'Group ID 1 is already assigned and cannot be used for the System group.', 1;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE ID = 0 AND Code = N'SAG')
    THROW 50011, 'The System group with ID 0 and code SAG was not found.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @OriginalCode nvarchar(20) =
        (SELECT Code FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK) WHERE ID = 0);
    DECLARE @TemporaryCode nvarchar(20) = N'#MIGRATING_SAG_0';

    IF EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE Code = @TemporaryCode AND ID <> 0)
        THROW 50013, 'The temporary System group migration code is already in use.', 1;

    UPDATE dbo.SystemGroups
    SET Code = @TemporaryCode
    WHERE ID = 0;

    SET IDENTITY_INSERT dbo.SystemGroups ON;

    INSERT INTO dbo.SystemGroups
    (
        ID,
        Code,
        EngName,
        ArbName,
        IsActive,
        RegDate,
        RegBy,
        ExpiredDate,
        CancellationDate,
        CancellationReason
    )
    SELECT
        1,
        @OriginalCode,
        EngName,
        ArbName,
        IsActive,
        RegDate,
        RegBy,
        ExpiredDate,
        CancellationDate,
        CancellationReason
    FROM dbo.SystemGroups
    WHERE ID = 0;

    SET IDENTITY_INSERT dbo.SystemGroups OFF;

    UPDATE dbo.SystemUsers
    SET GroupID = 1
    WHERE GroupID = 0;

    UPDATE dbo.SystemFormsPermissions
    SET GroupID = 1
    WHERE GroupID = 0;

    DELETE FROM dbo.SystemGroups
    WHERE ID = 0;

    IF NOT EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE ID = 1 AND Code = N'SAG')
       OR EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE ID = 0)
       OR EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE GroupID = 0)
       OR EXISTS (SELECT 1 FROM dbo.SystemFormsPermissions WHERE GroupID = 0)
        THROW 50012, 'System group ID migration verification failed.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    BEGIN TRY
        SET IDENTITY_INSERT dbo.SystemGroups OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    THROW;
END CATCH;
