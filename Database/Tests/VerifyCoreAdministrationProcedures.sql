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

EXEC dbo.usp_Add_SystemGroupPermissions
    @GroupID = @GroupID, @PermissionsJson = @Permissions, @RegUserID = 1,
    @HasError = @HasError OUTPUT, @ErrorDesc = @ErrorDesc OUTPUT;
IF @HasError = 1
    THROW 50995, 'The group permission could not be saved.', 1;

CREATE TABLE #GroupPermissions
(
    GroupID bigint, GroupCode varchar(100), GroupEnglishName varchar(200), GroupArabicName nvarchar(200),
    ModuleID bigint, ModuleCode varchar(100), ModuleEnglishName varchar(200), ModuleArabicName nvarchar(200),
    SubModuleID bigint, SubModuleCode varchar(100), SubModuleEnglishName varchar(200), SubModuleArabicName nvarchar(200),
    FormID bigint, FormCode varchar(200), FormEnglishName varchar(200), FormArabicName nvarchar(200),
    IsPermitted bit, CanView bit, PermissionID bigint, CanSave bit, CanUpdate bit, CanDelete bit, CanSearch bit, CanPrint bit
);
INSERT #GroupPermissions EXEC dbo.usp_Get_GroupPermissions @GroupID=@GroupID;
IF NOT EXISTS (SELECT 1 FROM #GroupPermissions WHERE FormID=1 AND IsPermitted=1 AND CanView=1 AND CanSave=0 AND CanSearch=1)
    THROW 50996, 'The saved group CanView permission was not returned correctly.', 1;
SELECT N'GroupPermissionRoundTrip' AS Test, CONVERT(bit,1) AS Passed;

EXEC dbo.usp_Add_SystemUserPermissions
    @UserID = @UserID, @GroupID = @GroupID, @PermissionsJson = @Permissions, @RegUserID = 1,
    @HasError = @HasError OUTPUT, @ErrorDesc = @ErrorDesc OUTPUT;
SELECT N'UserPermissionSave' AS Test, @HasError AS HasError;
IF NOT EXISTS
(
    SELECT 1 FROM dbo.SystemFormsPermissions
    WHERE UserID=@UserID AND GroupID IS NULL AND FormID=1 AND CanView=1 AND CanSave=0 AND CanSearch=1 AND CancelledDate IS NULL
)
    THROW 50997, 'The direct user permission was not stored independently from the group.', 1;

CREATE TABLE #EffectivePermissions
(
    ModuleID bigint, ModuleCode varchar(100), ModuleEnglishName varchar(200), ModuleArabicName nvarchar(200),
    SubModuleID bigint, SubModuleCode varchar(100), SubModuleEnglishName varchar(200), SubModuleArabicName nvarchar(200),
    FormID bigint, FormCode varchar(200), FormEnglishName varchar(200), FormArabicName nvarchar(200),
    CanView bit, CanSave bit, CanUpdate bit, CanDelete bit, CanSearch bit, CanPrint bit, HasUserOverride bit
);
INSERT #EffectivePermissions EXEC dbo.usp_Get_UserPermissions @UserID=@UserID,@GroupID=NULL;
IF NOT EXISTS (SELECT 1 FROM #EffectivePermissions WHERE FormID=1 AND CanView=1 AND CanSave=0 AND CanSearch=1 AND HasUserOverride=1)
    THROW 50998, 'The effective user permission did not preserve the saved CanView override.', 1;
SELECT N'EffectivePermissionRead' AS Test, CONVERT(bit,1) AS Passed;

ROLLBACK TRANSACTION;

IF EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE Code LIKE N'BP_TEST%')
    OR EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE Code LIKE N'BP_TEST%')
    THROW 50999, 'Test rollback failed; temporary records remain.', 1;

SELECT N'RollbackVerification' AS Test, CONVERT(bit, 1) AS Passed;
GO
