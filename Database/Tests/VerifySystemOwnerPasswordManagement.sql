USE [ARBP];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID=1 AND Code=N'Admin' AND IsSystemOwner=1 AND IsActive=1)
    THROW 50980, 'The Admin account is not the active system owner.', 1;
IF (SELECT COUNT(*) FROM dbo.SystemUsers WHERE IsSystemOwner=1) <> 1
    THROW 50981, 'Exactly one system owner is required.', 1;

BEGIN TRANSACTION;
DECLARE @TestUserID bigint, @HasError bit, @ErrorDesc nvarchar(2048);
EXEC dbo.usp_Add_SystemUser
    @GroupID=1, @Code=N'OWNER_SECURITY_TEST', @EngName=N'Owner Security Test', @IsActive=1,
    @RegBy=1, @HasError=@HasError OUTPUT, @ErrorDesc=@ErrorDesc OUTPUT,
    @NewUserID=@TestUserID OUTPUT,
    @PasswordHash=N'$pbkdf2-sha256$310000$dGVzdHNhbHQxMjM0NTY=$dGVzdGhhc2gxMjM0NTY3ODkwMTIzNDU2Nzg5MDE=';

EXEC dbo.usp_Reset_UserPassword
    @UserID=@TestUserID,
    @PasswordHash=N'$pbkdf2-sha256$310000$dGVzdHNhbHQxMjM0NTY=$bmV3dGVzdGhhc2gxMjM0NTY3ODkwMTIzNDU2Nw==',
    @PerformedByUserID=1;

IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID=@TestUserID AND MustChangePassword=1)
    THROW 50982, 'Password reset did not require a password change.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.SystemSecurityAudit WHERE TargetUserID=@TestUserID AND EventType='PasswordResetBySystemOwner')
    THROW 50983, 'Password reset was not audited.', 1;

BEGIN TRY
    EXEC dbo.usp_Delete_User @UserID=1;
    THROW 50984, 'The system owner could be deactivated.', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER()=50984 THROW;
END CATCH;

ROLLBACK TRANSACTION;
IF EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE Code=N'OWNER_SECURITY_TEST')
    THROW 50985, 'Security test rollback failed.', 1;
SELECT N'SystemOwnerPasswordManagement' AS Test, CONVERT(bit,1) AS Passed;
GO
