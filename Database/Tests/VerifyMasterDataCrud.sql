SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @ID bigint;
EXEC dbo.usp_MasterData_Save
    @EntityKey='Gender',
    @Payload=N'{"Code":"TEST","EngName":"Test","ArbName":"اختبار","IsActive":true}',
    @RegUserID=1,
    @SavedID=@ID OUTPUT;

IF @ID IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.Gender WHERE ID=@ID AND Code=N'TEST' AND IsActive=1)
    THROW 50200, 'Master-data insert verification failed.', 1;

EXEC dbo.usp_MasterData_Save
    @EntityKey='Gender',
    @ID=@ID,
    @Payload=N'{"Code":"TEST","EngName":"Updated test","ArbName":"اختبار محدث","IsActive":true}',
    @RegUserID=1,
    @SavedID=@ID OUTPUT;

IF NOT EXISTS (SELECT 1 FROM dbo.Gender WHERE ID=@ID AND EngName='Updated test')
    THROW 50201, 'Master-data update verification failed.', 1;

EXEC dbo.usp_MasterData_Delete @EntityKey='Gender',@ID=@ID,@CancelUserID=1;
IF NOT EXISTS (SELECT 1 FROM dbo.Gender WHERE ID=@ID AND IsActive=0 AND CancelUserID=1)
    THROW 50202, 'Master-data deactivation verification failed.', 1;

ROLLBACK TRANSACTION;

EXECUTE AS USER = 'ARBP_AppUser';
BEGIN TRANSACTION;
DECLARE @AppUserID bigint;
EXEC dbo.usp_MasterData_Save @EntityKey='Gender', @Payload=N'{"Code":"APPTEST","EngName":"App permission test","ArbName":"اختبار صلاحيات التطبيق","IsActive":true}', @RegUserID=1, @SavedID=@AppUserID OUTPUT;
EXEC dbo.usp_MasterData_GetRecord @EntityKey='Gender', @ID=@AppUserID;
EXEC dbo.usp_MasterData_Delete @EntityKey='Gender', @ID=@AppUserID, @CancelUserID=1;
ROLLBACK TRANSACTION;
REVERT;

SELECT 'Master-data CRUD verification passed; test data was rolled back.' AS Result;
