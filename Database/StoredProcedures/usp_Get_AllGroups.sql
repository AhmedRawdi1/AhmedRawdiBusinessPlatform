CREATE OR ALTER PROCEDURE dbo.usp_Get_AllGroups
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ID          AS GroupID,
        Code        AS GroupCode,
        EngName     AS GroupEnglishName,
        ArbName     AS GroupArabicName,
        IsActive,
        RegDate,
        ExpiredDate
    FROM dbo.SystemGroups
    ORDER BY IsActive DESC, EngName, Code;
END;
