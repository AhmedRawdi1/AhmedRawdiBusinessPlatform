SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.usp_Get_AllGroups', N'P') IS NULL
   AND OBJECT_ID(N'dbo.usp_GetAllGroups', N'P') IS NOT NULL
    EXEC sys.sp_rename N'dbo.usp_GetAllGroups', N'usp_Get_AllGroups', N'OBJECT';

IF OBJECT_ID(N'dbo.usp_Get_GroupPermissions', N'P') IS NULL
   AND OBJECT_ID(N'dbo.usp_GetGroupPermissions', N'P') IS NOT NULL
    EXEC sys.sp_rename N'dbo.usp_GetGroupPermissions', N'usp_Get_GroupPermissions', N'OBJECT';

IF OBJECT_ID(N'dbo.usp_Get_UserPermissions', N'P') IS NULL
   AND OBJECT_ID(N'dbo.usp_GetUserPermissions', N'P') IS NOT NULL
    EXEC sys.sp_rename N'dbo.usp_GetUserPermissions', N'usp_Get_UserPermissions', N'OBJECT';

COMMIT TRANSACTION;
