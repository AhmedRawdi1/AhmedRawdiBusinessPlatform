SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID = 1)
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID = 1 AND Code = N'Admin')
       AND NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID = 0)
        RETURN;

    THROW 50020, 'User ID 1 is already assigned and cannot be used for the System user.', 1;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID = 0 AND Code = N'Admin')
    THROW 50021, 'The System user with ID 0 and code Admin was not found.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @OriginalCode nvarchar(200) =
        (SELECT Code FROM dbo.SystemUsers WITH (UPDLOCK, HOLDLOCK) WHERE ID = 0);
    DECLARE @TemporaryCode nvarchar(200) = N'#MIGRATING_ADMIN_0';

    IF EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE Code = @TemporaryCode AND ID <> 0)
        THROW 50022, 'The temporary System user migration code is already in use.', 1;

    UPDATE dbo.SystemUsers
    SET Code = @TemporaryCode
    WHERE ID = 0;

    SET IDENTITY_INSERT dbo.SystemUsers ON;

    INSERT INTO dbo.SystemUsers
    (
        ID,
        GroupID,
        Code,
        EngName,
        ArbName,
        IsActive,
        UserPass,
        Email,
        MobileNum,
        PreferredLanguage,
        RegDate,
        RegBy,
        ExpiredDate,
        CancellationDate,
        CancellationReason
    )
    SELECT
        1,
        GroupID,
        @OriginalCode,
        EngName,
        ArbName,
        IsActive,
        UserPass,
        Email,
        MobileNum,
        PreferredLanguage,
        RegDate,
        RegBy,
        ExpiredDate,
        CancellationDate,
        CancellationReason
    FROM dbo.SystemUsers
    WHERE ID = 0;

    SET IDENTITY_INSERT dbo.SystemUsers OFF;

    UPDATE dbo.Branches SET RegBy = 1 WHERE RegBy = 0;
    UPDATE dbo.Companies SET RegBy = 1 WHERE RegBy = 0;
    UPDATE dbo.IdentityTypes SET RegUserID = 1 WHERE RegUserID = 0;
    UPDATE dbo.IdentityTypes SET CancelUserID = 1 WHERE CancelUserID = 0;
    UPDATE dbo.MaritalStatuses SET RegUserID = 1 WHERE RegUserID = 0;
    UPDATE dbo.MaritalStatuses SET CancelUserID = 1 WHERE CancelUserID = 0;
    UPDATE dbo.PatientFileTypes SET RegUserID = 1 WHERE RegUserID = 0;
    UPDATE dbo.PatientFileTypes SET CancelUserID = 1 WHERE CancelUserID = 0;
    UPDATE dbo.Patients SET RegUserID = 1 WHERE RegUserID = 0;
    UPDATE dbo.Patients SET CancelUserID = 1 WHERE CancelUserID = 0;
    UPDATE dbo.Relations SET RegUserID = 1 WHERE RegUserID = 0;
    UPDATE dbo.Relations SET CancelUserID = 1 WHERE CancelUserID = 0;
    UPDATE dbo.SystemFormsPermissions SET UserID = 1 WHERE UserID = 0;
    UPDATE dbo.SystemFormsPermissions SET RegUserID = 1 WHERE RegUserID = 0;
    UPDATE dbo.SystemFormsPermissions SET CancelledUserID = 1 WHERE CancelledUserID = 0;
    UPDATE dbo.SystemGroups SET RegBy = 1 WHERE RegBy = 0;
    UPDATE dbo.SystemUsers SET RegBy = 1 WHERE RegBy = 0;

    DELETE FROM dbo.SystemUsers
    WHERE ID = 0;

    IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID = 1 AND Code = N'Admin')
       OR EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID = 0)
       OR EXISTS (SELECT 1 FROM dbo.Branches WHERE RegBy = 0)
       OR EXISTS (SELECT 1 FROM dbo.Companies WHERE RegBy = 0)
       OR EXISTS (SELECT 1 FROM dbo.IdentityTypes WHERE RegUserID = 0 OR CancelUserID = 0)
       OR EXISTS (SELECT 1 FROM dbo.MaritalStatuses WHERE RegUserID = 0 OR CancelUserID = 0)
       OR EXISTS (SELECT 1 FROM dbo.PatientFileTypes WHERE RegUserID = 0 OR CancelUserID = 0)
       OR EXISTS (SELECT 1 FROM dbo.Patients WHERE RegUserID = 0 OR CancelUserID = 0)
       OR EXISTS (SELECT 1 FROM dbo.Relations WHERE RegUserID = 0 OR CancelUserID = 0)
       OR EXISTS (SELECT 1 FROM dbo.SystemFormsPermissions WHERE UserID = 0 OR RegUserID = 0 OR CancelledUserID = 0)
       OR EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE RegBy = 0)
       OR EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE RegBy = 0)
        THROW 50023, 'System user ID migration verification failed.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    BEGIN TRY
        SET IDENTITY_INSERT dbo.SystemUsers OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    THROW;
END CATCH;
