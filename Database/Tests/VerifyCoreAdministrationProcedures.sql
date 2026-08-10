USE [ARBP];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @GroupID bigint;
DECLARE @UserID bigint;
DECLARE @HasError bit;
DECLARE @ErrorDesc nvarchar(2048);

EXEC dbo.usp_Add_SystemGroups
    @Code = N'BP_TEST_GRP', @EngName = N'Best Practice Test Group', @ArbName = N'Best Practice Test Group',
    @IsActive = 1, @RegBy = 1, @NewGroupID = @GroupID OUTPUT,
    @HasError = @HasError OUTPUT, @ErrorDesc = @ErrorDesc OUTPUT;
SELECT N'GroupInsert' AS Test, @HasError AS HasError, CONVERT(bit, CASE WHEN @GroupID IS NULL THEN 0 ELSE 1 END) AS ReturnedID;

EXEC dbo.usp_Add_SystemGroups
    @Code = N'BP_TEST_GRP_2', @EngName = N'Updated Test Group', @ArbName = N'Updated Test Group',
    @IsActive = 1, @RegBy = 1, @NewGroupID = @GroupID OUTPUT,
    @HasError = @HasError OUTPUT, @ErrorDesc = @ErrorDesc OUTPUT, @GroupID = @GroupID;
SELECT N'GroupUpdate' AS Test, @HasError AS HasError;

EXEC dbo.usp_Add_SystemUser
    @GroupID = @GroupID, @Code = N'BP_TEST_USER', @EngName = N'Best Practice Test User',
    @ArbName = N'Best Practice Test User', @IsActive = 1, @RegBy = 1,
    @HasError = @HasError OUTPUT, @ErrorDesc = @ErrorDesc OUTPUT, @NewUserID = @UserID OUTPUT,
    @PasswordHash = N'$pbkdf2-sha256$310000$dGVzdHNhbHQxMjM0NTY=$dGVzdGhhc2gxMjM0NTY3ODkwMTIzNDU2Nzg5MDE=';
SELECT N'UserInsert' AS Test, @HasError AS HasError, CONVERT(bit, CASE WHEN @UserID IS NULL THEN 0 ELSE 1 END) AS ReturnedID;

EXEC dbo.usp_Add_SystemUser
    @GroupID = @GroupID, @Code = N'BP_TEST_USER_2', @EngName = N'Updated Test User',
    @ArbName = N'Updated Test User', @IsActive = 1, @RegBy = 1,
    @HasError = @HasError OUTPUT, @ErrorDesc = @ErrorDesc OUTPUT,
    @UserID = @UserID, @NewUserID = @UserID OUTPUT;
SELECT N'UserUpdate' AS Test, @HasError AS HasError;

DECLARE @Permissions nvarchar(max) = N'[{"FormID":1,"CanView":true,"CanSave":false,"CanUpdate":false,"CanDelete":false,"CanSearch":true,"CanPrint":false}]';

EXEC dbo.usp_Add_SystemUserPermissions
    @UserID = @UserID, @GroupID = @GroupID, @PermissionsJson = @Permissions, @RegUserID = 1,
    @HasError = @HasError OUTPUT, @ErrorDesc = @ErrorDesc OUTPUT;
SELECT N'UserPermissionSave' AS Test, @HasError AS HasError;

ROLLBACK TRANSACTION;

IF EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE Code LIKE N'BP_TEST%')
    OR EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE Code LIKE N'BP_TEST%')
    THROW 50999, 'Test rollback failed; temporary records remain.', 1;

SELECT N'RollbackVerification' AS Test, CONVERT(bit, 1) AS Passed;
GO
