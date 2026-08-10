CREATE OR ALTER PROCEDURE dbo.usp_MasterData_GetRecord
    @EntityKey varchar(50),
    @ID bigint
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TableName sysname = CASE @EntityKey
        WHEN 'Companies' THEN 'Companies'
        WHEN 'Branches' THEN 'Branches'
        WHEN 'CostCentersGroup' THEN 'CostCentersGroup'
        WHEN 'CostCentersTypes' THEN 'CostCentersTypes'
        WHEN 'CostCenters' THEN 'CostCenters'
        WHEN 'Nationalities' THEN 'Nationalities'
        WHEN 'IdentityTypes' THEN 'IdentityTypes'
        WHEN 'MaritalStatuses' THEN 'MaritalStatuses'
        WHEN 'Gender' THEN 'Gender'
        WHEN 'MedicalSpecialties' THEN 'MedicalSpecialties'
        WHEN 'PhysiciansLevels' THEN 'PhysiciansLevels'
        WHEN 'Physicians' THEN 'Physicians'
    END;

    IF @TableName IS NULL THROW 50100, 'Unsupported master-data entity.', 1;

    DECLARE @Sql nvarchar(max) = N'SELECT (SELECT * FROM dbo.' + QUOTENAME(@TableName)
        + N' WHERE ID = @RecordID FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES)';
    EXEC sys.sp_executesql @Sql, N'@RecordID bigint', @RecordID = @ID;
END;
