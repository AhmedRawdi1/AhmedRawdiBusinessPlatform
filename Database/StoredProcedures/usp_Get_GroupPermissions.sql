CREATE OR ALTER PROCEDURE dbo.usp_Get_GroupPermissions
    @GroupID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF @GroupID IS NULL
        THROW 50001, 'GroupID is required.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE ID = @GroupID)
        THROW 50002, 'The selected group does not exist.', 1;

    SELECT
        g.ID                    AS GroupID,
        g.Code                  AS GroupCode,
        g.EngName               AS GroupEnglishName,
        g.ArbName               AS GroupArabicName,
        m.SysModID              AS ModuleID,
        m.SysModCode            AS ModuleCode,
        m.SysModEngName         AS ModuleEnglishName,
        m.SysModArbName         AS ModuleArabicName,
        sm.SysSubModID          AS SubModuleID,
        sm.SysSubModCode        AS SubModuleCode,
        sm.SysSubModEngName     AS SubModuleEnglishName,
        sm.SysSubModArbName     AS SubModuleArabicName,
        f.FormID,
        f.FormCode,
        f.FormEngName           AS FormEnglishName,
        f.FormArbName           AS FormArabicName,
        CAST(CASE WHEN permission.ID IS NULL THEN 0 ELSE 1 END AS BIT) AS IsPermitted,
        CAST(CASE WHEN permission.ID IS NULL THEN 0 ELSE 1 END AS BIT) AS CanView,
        permission.ID           AS PermissionID,
        CAST(ISNULL(permission.CanSave, 0) AS BIT)   AS CanSave,
        CAST(ISNULL(permission.CanUpdate, 0) AS BIT) AS CanUpdate,
        CAST(ISNULL(permission.CanDelete, 0) AS BIT) AS CanDelete,
        CAST(ISNULL(permission.CanSearch, 0) AS BIT) AS CanSearch,
        CAST(ISNULL(permission.CanPrint, 0) AS BIT)  AS CanPrint
    FROM dbo.SystemGroups g
    CROSS JOIN dbo.SystemForms f
    INNER JOIN dbo.SystemSubModules sm ON sm.SysSubModID = f.SubModID
    INNER JOIN dbo.SystemModules m ON m.SysModID = sm.SysModID
    OUTER APPLY
    (
        SELECT TOP (1)
            p.ID,
            p.CanSave,
            p.CanUpdate,
            p.CanDelete,
            p.CanSearch,
            p.CanPrint
        FROM dbo.SystemFormsPermissions p
        WHERE p.GroupID = g.ID
          AND p.FormID = f.FormID
          AND p.UserID IS NULL
          AND p.CancelledDate IS NULL
        ORDER BY p.RegDate DESC, p.ID DESC
    ) permission
    WHERE g.ID = @GroupID
      AND f.IsActive = 1
      AND sm.IsActive = 1
      AND m.IsActive = 1
    ORDER BY
        ISNULL(m.ModuleRank, 2147483647), m.SysModEngName,
        ISNULL(sm.SubModuleRank, 2147483647), sm.SysSubModEngName,
        ISNULL(f.FormRank, 2147483647), f.FormEngName;
END;
