USE [ARBP];
GO

CREATE OR ALTER PROCEDURE dbo.usp_Get_AllUsers
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.ID AS UserID,
        u.GroupID,
        u.Code AS UserCode,
        u.EngName AS UserEnglishName,
        u.ArbName AS UserArabicName,
        u.Email,
        u.MobileNum,
        u.PreferredLanguage,
        ISNULL(u.IsActive, 0) AS IsActive,
        u.RegDate,
        u.ExpiredDate,
        g.Code AS GroupCode,
        g.EngName AS GroupEnglishName,
        g.ArbName AS GroupArabicName
    FROM dbo.SystemUsers u
    INNER JOIN dbo.SystemGroups g ON g.ID = u.GroupID
    ORDER BY ISNULL(u.IsActive, 0) DESC, u.EngName, u.Code;
END;
GO
