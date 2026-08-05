CREATE OR ALTER PROCEDURE dbo.usp_Get_AllSystemForms
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.SysModID AS ModuleID,
        m.SysModCode AS ModuleCode,
        m.SysModEngName AS ModuleEnglishName,
        m.SysModArbName AS ModuleArabicName,
        m.IsActive AS ModuleIsActive,
        m.ModuleRank,
        m.CreatedON AS ModuleCreatedOn,
        m.CancelledOn AS ModuleCancelledOn,
        sm.SysSubModID AS SubModuleID,
        sm.SysSubModCode AS SubModuleCode,
        sm.SysSubModEngName AS SubModuleEnglishName,
        sm.SysSubModArbName AS SubModuleArabicName,
        sm.IsActive AS SubModuleIsActive,
        sm.SubModuleRank,
        sm.DescriptionEN AS SubModuleDescriptionEnglish,
        sm.DescriptionAR AS SubModuleDescriptionArabic,
        sm.CreatedON AS SubModuleCreatedOn,
        sm.CancelledOn AS SubModuleCancelledOn,
        f.FormID,
        f.FormCode,
        f.FormEngName AS FormEnglishName,
        f.FormArbName AS FormArabicName,
        f.IsActive AS FormIsActive,
        f.FormRank,
        f.CreatedOn AS FormCreatedOn,
        f.CancelledOn AS FormCancelledOn,
        CAST(0 AS bit) AS CanView,
        CAST(0 AS bit) AS CanSave,
        CAST(0 AS bit) AS CanDelete,
        CAST(0 AS bit) AS CanSearch,
        CAST(0 AS bit) AS CanPrint
    FROM dbo.SystemForms AS f
    INNER JOIN dbo.SystemSubModules AS sm
        ON sm.SysSubModID = f.SubModID
    LEFT JOIN dbo.SystemModules AS m
        ON m.SysModID = sm.SysModID
    ORDER BY
        CASE WHEN m.ModuleRank IS NULL THEN 1 ELSE 0 END,
        m.ModuleRank,
        m.SysModEngName,
        CASE WHEN sm.SubModuleRank IS NULL THEN 1 ELSE 0 END,
        sm.SubModuleRank,
        sm.SysSubModEngName,
        CASE WHEN f.FormRank IS NULL THEN 1 ELSE 0 END,
        f.FormRank,
        f.FormEngName;
END;
