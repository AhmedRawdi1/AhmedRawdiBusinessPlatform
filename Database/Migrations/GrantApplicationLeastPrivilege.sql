USE [ARBP];
GO

-- The server login and its password must be provisioned by the deployment platform.
-- Override AppLogin with sqlcmd -v AppLogin="YourServiceLogin" when required.
:setvar AppLogin "ARBP_AppUser"

IF SUSER_ID(N'$(AppLogin)') IS NULL
    THROW 50110, 'The application server login must be created before granting database access.', 1;

IF USER_ID(N'$(AppLogin)') IS NULL
BEGIN
    DECLARE @CreateUserSql nvarchar(max) = N'CREATE USER ' + QUOTENAME(N'$(AppLogin)') + N' FOR LOGIN ' + QUOTENAME(N'$(AppLogin)') + N';';
    EXEC sys.sp_executesql @CreateUserSql;
END;
GO

DECLARE @ApplicationUser sysname = N'$(AppLogin)';
DECLARE @GrantSql nvarchar(max) = N'';

SELECT @GrantSql +=
    N'GRANT EXECUTE ON OBJECT::' + QUOTENAME(OBJECT_SCHEMA_NAME(p.object_id)) + N'.' + QUOTENAME(p.name)
    + N' TO ' + QUOTENAME(@ApplicationUser) + N';' + CHAR(13) + CHAR(10)
FROM sys.procedures p
WHERE p.name IN
(
    N'usp_Get_UserInfo', N'usp_Get_UserPermissions',
    N'usp_Get_AllGroups', N'usp_Get_GroupPermissions', N'usp_Get_AllSystemForms',
    N'usp_Add_SystemGroups', N'usp_Add_SystemGroupPermissions', N'usp_Delete_Group',
    N'usp_Get_AllUsers', N'usp_Add_SystemUser', N'usp_Add_SystemUserPermissions', N'usp_Delete_User',
    N'usp_Reset_UserPassword', N'usp_Change_OwnPassword', N'usp_Validate_UserSession'
);

EXEC sys.sp_executesql @GrantSql;
GO
