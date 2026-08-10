CREATE OR ALTER PROCEDURE dbo.usp_MasterData_Search
    @EntityKey varchar(50),
    @Search nvarchar(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @Search = NULLIF(LTRIM(RTRIM(@Search)), N'');

    DECLARE @Records TABLE
    (
        ID bigint NOT NULL,
        Code nvarchar(150) NOT NULL,
        EnglishName nvarchar(200) NOT NULL,
        ArabicName nvarchar(200) NOT NULL,
        IsActive bit NOT NULL
    );

    IF @EntityKey = 'Companies'
        INSERT @Records SELECT ID, Code, EngName, ArbName, IsActive FROM dbo.Companies;
    ELSE IF @EntityKey = 'Branches'
        INSERT @Records SELECT ID, Code, EngName, ArbName, ISNULL(IsActive, 1) FROM dbo.Branches;
    ELSE IF @EntityKey = 'CostCentersGroup'
        INSERT @Records SELECT ID, Code, EngName, ArbName, CONVERT(bit, CASE WHEN CancelDate IS NULL THEN 1 ELSE 0 END) FROM dbo.CostCentersGroup;
    ELSE IF @EntityKey = 'CostCentersTypes'
        INSERT @Records SELECT ID, Code, EngName, ArbName, CONVERT(bit, CASE WHEN CancelDate IS NULL THEN 1 ELSE 0 END) FROM dbo.CostCentersTypes;
    ELSE IF @EntityKey = 'CostCentersLevels'
        INSERT @Records SELECT ID, Code, EngName, ArbName, CONVERT(bit, CASE WHEN CancelDate IS NULL THEN 1 ELSE 0 END) FROM dbo.CostCentersLevels;
    ELSE IF @EntityKey = 'CostCenters'
        INSERT @Records SELECT ID, Code, EngName, ArbName, CONVERT(bit, CASE WHEN CancelDate IS NULL THEN 1 ELSE 0 END) FROM dbo.CostCenters;
    ELSE IF @EntityKey = 'Nationalities'
        INSERT @Records SELECT ID, Code, Nat_EngName, Nat_ArbName, IsActive FROM dbo.Nationalities;
    ELSE IF @EntityKey = 'IdentityTypes'
        INSERT @Records SELECT ID, Code, EngName, ArbName, CONVERT(bit, CASE WHEN CancelDate IS NULL THEN 1 ELSE 0 END) FROM dbo.IdentityTypes;
    ELSE IF @EntityKey = 'MaritalStatuses'
        INSERT @Records SELECT ID, Code, EngName, ArbName, CONVERT(bit, CASE WHEN CancelDate IS NULL THEN 1 ELSE 0 END) FROM dbo.MaritalStatuses;
    ELSE IF @EntityKey = 'Gender'
        INSERT @Records SELECT CONVERT(bigint, ID), ISNULL(Code, N''), ISNULL(EngName, N''), ISNULL(ArbName, N''), IsActive FROM dbo.Gender;
    ELSE IF @EntityKey = 'MedicalSpecialties'
        INSERT @Records SELECT ID, Code, EngName, ArbName, CONVERT(bit, CASE WHEN CancelDate IS NULL THEN 1 ELSE 0 END) FROM dbo.MedicalSpecialties;
    ELSE IF @EntityKey = 'PhysiciansLevels'
        INSERT @Records SELECT ID, Code, EngName, ArbName, CONVERT(bit, CASE WHEN CancelDate IS NULL THEN 1 ELSE 0 END) FROM dbo.PhysiciansLevels;
    ELSE IF @EntityKey = 'Physicians'
        INSERT @Records SELECT ID, Code, EngName, ArbName, IsActive FROM dbo.Physicians;
    ELSE
        THROW 50100, 'Unsupported master-data entity.', 1;

    SELECT TOP (200) ID, Code, EnglishName, ArabicName, IsActive
    FROM @Records
    WHERE @Search IS NULL
       OR Code LIKE N'%' + @Search + N'%'
       OR EnglishName LIKE N'%' + @Search + N'%'
       OR ArabicName LIKE N'%' + @Search + N'%'
    ORDER BY IsActive DESC, Code, ID;
END;
