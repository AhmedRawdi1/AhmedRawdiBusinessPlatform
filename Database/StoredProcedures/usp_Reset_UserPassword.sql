USE [ARBP];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Reset_UserPassword
    @UserID bigint,
    @PasswordHash nvarchar(512),
    @PerformedByUserID bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @PasswordHash = NULLIF(LTRIM(RTRIM(@PasswordHash)), N'');
    IF @PasswordHash IS NULL THROW 50210, 'A password hash is required.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID=@PerformedByUserID AND IsSystemOwner=1 AND IsActive=1 AND CancellationDate IS NULL)
        THROW 50211, 'Only the system owner can reset user passwords.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID=@UserID AND CancellationDate IS NULL)
        THROW 50212, 'The selected user does not exist.', 1;
    IF EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID=@UserID AND IsSystemOwner=1)
        THROW 50213, 'The system owner password cannot be reset from user management.', 1;

    BEGIN TRANSACTION;
    UPDATE dbo.SystemUsers
    SET PasswordHash=@PasswordHash,
        PasswordChangedAt=SYSUTCDATETIME(),
        MustChangePassword=1,
        SecurityStamp=NEWID()
    WHERE ID=@UserID;

    INSERT dbo.SystemSecurityAudit (ID, EventType, TargetUserID, PerformedByUserID, Details)
    VALUES (NEXT VALUE FOR dbo.Seq_SystemSecurityAuditID, 'PasswordResetBySystemOwner', @UserID, @PerformedByUserID, N'Password reset; change required at next sign-in; previous sessions invalidated.');
    COMMIT TRANSACTION;
END;
GO
