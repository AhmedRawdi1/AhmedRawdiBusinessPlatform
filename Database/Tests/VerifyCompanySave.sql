USE [ARBP];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @SavedID bigint;
    EXEC dbo.usp_MasterData_Save
        @EntityKey='Companies',
        @ID=NULL,
        @Payload=N'{"Code":"DIAG-CO","EngName":"Diagnostic Company","ArbName":"Diagnostic Company","CRN":"123","VATRN":"456","PhoneNo":"0500000000","Email":"test@example.com","LocationCountryID":1,"LocationCityID":1,"AddressEngName":"Address","AddressArbName":"Address","IsActive":true}',
        @RegUserID=1,
        @SavedID=@SavedID OUTPUT;

    IF NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE ID=@SavedID AND Code=N'DIAG-CO' AND IsActive=1)
        THROW 50970, 'Company save verification failed.', 1;

    SELECT N'CompanySave' AS Test, @SavedID AS SavedID, CONVERT(bit,1) AS Passed;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
