CREATE OR ALTER PROCEDURE [dbo].[usp_Get_UserPermissions]
    @GroupID bigint = NULL,
    @UserID bigint = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @EffectiveGroupID bigint;
    DECLARE @UserGroupID bigint;

    IF @GroupID IS NULL AND @UserID IS NULL
        THROW 50010, 'At least one of GroupID or UserID is required.', 1;

    IF @GroupID IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM dbo.SystemGroups
           WHERE ID = @GroupID
             AND IsActive = 1
             AND CancellationDate IS NULL
             AND (ExpiredDate IS NULL OR ExpiredDate >= GETDATE())
       )
        THROW 50011, 'The supplied group is not active or does not exist.', 1;

    IF @UserID IS NOT NULL
    BEGIN
        SELECT @UserGroupID = su.GroupID
        FROM dbo.SystemUsers AS su
        WHERE su.ID = @UserID
          AND su.IsActive = 1
          AND su.CancellationDate IS NULL
          AND (su.ExpiredDate IS NULL OR su.ExpiredDate >= GETDATE())

        IF @UserGroupID IS NULL
            SELECT @UserGroupID = @GroupID;
    END;

    SET @EffectiveGroupID = COALESCE(@GroupID, @UserGroupID);

    ;WITH RankedGroupPermissions AS
    (
        SELECT
            sfp.*,
            ROW_NUMBER() OVER
            (
                PARTITION BY sfp.FormID
                ORDER BY sfp.RegDate DESC, sfp.ID DESC
            ) AS RowNumber
        FROM dbo.SystemFormsPermissions AS sfp
        WHERE sfp.GroupID = @EffectiveGroupID
          AND sfp.UserID IS NULL
          AND sfp.CancelledDate IS NULL
    ),
    GroupPermissions AS
    (
        SELECT FormID, CanSave, CanUpdate, CanDelete, CanSearch, CanPrint
        FROM RankedGroupPermissions
        WHERE RowNumber = 1
    ),
    RankedUserPermissions AS
    (
        SELECT
            sfp.*,
            ROW_NUMBER() OVER
            (
                PARTITION BY sfp.FormID
                ORDER BY sfp.RegDate DESC, sfp.ID DESC
            ) AS RowNumber
        FROM dbo.SystemFormsPermissions AS sfp
        WHERE @UserID IS NOT NULL
          AND sfp.UserID = @UserID
          AND sfp.CancelledDate IS NULL
    ),
    UserPermissions AS
    (
        SELECT FormID, CanSave, CanUpdate, CanDelete, CanSearch, CanPrint
        FROM RankedUserPermissions
        WHERE RowNumber = 1
    ),
    PermissionFormIds AS
    (
        SELECT FormID 
        FROM dbo.SystemForms 
        WHERE IsActive = 1 AND CancelledOn IS NULL
    ),
    EffectivePermissions AS
    (
        SELECT
            ids.FormID,
            CONVERT(bit, CASE 
                WHEN up.FormID IS NOT NULL THEN (CASE WHEN up.CanSave=1 OR up.CanUpdate=1 OR up.CanDelete=1 OR up.CanSearch=1 OR up.CanPrint=1 THEN 1 ELSE 0 END)
                WHEN gp.FormID IS NOT NULL THEN (CASE WHEN gp.CanSave=1 OR gp.CanUpdate=1 OR gp.CanDelete=1 OR gp.CanSearch=1 OR gp.CanPrint=1 THEN 1 ELSE 0 END)
                ELSE 0 
            END) AS CanView,
            CONVERT(bit, COALESCE(up.CanSave, gp.CanSave, 0)) AS CanSave,
            CONVERT(bit, COALESCE(up.CanUpdate, gp.CanUpdate, 0)) AS CanUpdate,
            CONVERT(bit, COALESCE(up.CanDelete, gp.CanDelete, 0)) AS CanDelete,
            CONVERT(bit, COALESCE(up.CanSearch, gp.CanSearch, 0)) AS CanSearch,
            CONVERT(bit, COALESCE(up.CanPrint, gp.CanPrint, 0)) AS CanPrint,
            CONVERT(bit, CASE WHEN up.FormID IS NULL THEN 0 ELSE 1 END) AS HasUserOverride
        FROM PermissionFormIds AS ids
        LEFT JOIN GroupPermissions AS gp ON gp.FormID = ids.FormID
        LEFT JOIN UserPermissions AS up ON up.FormID = ids.FormID
    )
    SELECT
        sm.SysModID AS ModuleID,
        sm.SysModCode AS ModuleCode,
        sm.SysModEngName AS ModuleEnglishName,
        sm.SysModArbName AS ModuleArabicName,
        ssm.SysSubModID AS SubModuleID,
        ssm.SysSubModCode AS SubModuleCode,
        ssm.SysSubModEngName AS SubModuleEnglishName,
        ssm.SysSubModArbName AS SubModuleArabicName,
        sf.FormID,
        sf.FormCode,
        sf.FormEngName AS FormEnglishName,
        sf.FormArbName AS FormArabicName,
        ep.CanView,
        ep.CanSave,
        ep.CanUpdate,
        ep.CanDelete,
        ep.CanSearch,
        ep.CanPrint,
        ep.HasUserOverride
    FROM EffectivePermissions AS ep
    INNER JOIN dbo.SystemForms AS sf
        ON sf.FormID = ep.FormID
       AND sf.IsActive = 1
       AND sf.CancelledOn IS NULL
    INNER JOIN dbo.SystemSubModules AS ssm
        ON ssm.SysSubModID = sf.SubModID
       AND ssm.IsActive = 1
       AND ssm.CancelledOn IS NULL
    INNER JOIN dbo.SystemModules AS sm
        ON sm.SysModID = ssm.SysModID
       AND sm.IsActive = 1
       AND sm.CancelledOn IS NULL
    ORDER BY
        COALESCE(sm.ModuleRank, 2147483647),
        COALESCE(ssm.SubModuleRank, 2147483647),
        COALESCE(sf.FormRank, 2147483647),
        sf.FormID;
END;
