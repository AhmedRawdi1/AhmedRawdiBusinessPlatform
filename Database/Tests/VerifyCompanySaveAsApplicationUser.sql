USE [ARBP];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @SavedID bigint;
    EXECUTE AS USER=N'ARBP_AppUser';
    EXEC dbo.usp_MasterData_Save
        @EntityKey='Companies',
        @ID=NULL,
        @Payload=N'{"Code":"APP-DIAG-CO","EngName":"Application Diagnostic Company","ArbName":"Application Diagnostic Company","CRN":"123","VATRN":"456","PhoneNo":"0500000000","Email":"test@example.com","CountryID":1,"AddressEngName":"Address","AddressArbName":"Address","IsActive":true}',
        @RegUserID=1,
        @SavedID=@SavedID OUTPUT;
    REVERT;

    IF NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE ID=@SavedID AND Code=N'APP-DIAG-CO')
        THROW 50969, 'Application-user company save verification failed.', 1;
    SELECT N'CompanySaveAsApplicationUser' AS Test,@SavedID SavedID,CONVERT(bit,1) Passed;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF USER_NAME()=N'ARBP_AppUser' REVERT;
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
