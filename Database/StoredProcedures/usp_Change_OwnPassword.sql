USE [ARBP];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Change_OwnPassword
    @UserID bigint,
    @PasswordHash nvarchar(512)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @PasswordHash = NULLIF(LTRIM(RTRIM(@PasswordHash)), N'');
    IF @PasswordHash IS NULL THROW 50220, 'A password hash is required.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID=@UserID AND IsActive=1 AND CancellationDate IS NULL)
        THROW 50221, 'The user account is not active.', 1;

    BEGIN TRANSACTION;
    UPDATE dbo.SystemUsers
    SET PasswordHash=@PasswordHash, PasswordChangedAt=SYSUTCDATETIME(), MustChangePassword=0, SecurityStamp=NEWID()
    WHERE ID=@UserID;
    INSERT dbo.SystemSecurityAudit (ID, EventType, TargetUserID, PerformedByUserID, Details)
    VALUES (NEXT VALUE FOR dbo.Seq_SystemSecurityAuditID, 'OwnPasswordChanged', @UserID, @UserID, N'User changed password; previous sessions invalidated.');
    COMMIT TRANSACTION;
END;
GO
