SET XACT_ABORT ON;

:r Database\Migrations\AddMasterDataLifecycleColumns.sql
GO
:r Database\StoredProcedures\usp_MasterData_Search.sql
GO
:r Database\StoredProcedures\usp_MasterData_GetRecord.sql
GO
:r Database\StoredProcedures\usp_MasterData_Save.sql
GO
:r Database\StoredProcedures\usp_MasterData_Delete.sql
GO

IF USER_ID(N'ARBP_AppUser') IS NOT NULL
BEGIN
    GRANT EXECUTE ON OBJECT::dbo.usp_MasterData_Search TO ARBP_AppUser;
    GRANT EXECUTE ON OBJECT::dbo.usp_MasterData_GetRecord TO ARBP_AppUser;
    GRANT EXECUTE ON OBJECT::dbo.usp_MasterData_Save TO ARBP_AppUser;
    GRANT EXECUTE ON OBJECT::dbo.usp_MasterData_Delete TO ARBP_AppUser;
END;
