CREATE OR ALTER PROCEDURE dbo.usp_Get_SystemGroupMembers
    @GroupID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF @GroupID IS NULL
        THROW 50001, 'GroupID is required.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE ID = @GroupID)
        THROW 50002, 'The selected group does not exist.', 1;

    SELECT
        u.ID                    AS UserID,
        u.Code                  AS UserCode,
        u.EngName               AS UserEnglishName,
        u.ArbName               AS UserArabicName,
        u.Email                 AS Email,
        u.MobileNum             AS MobileNum,
        u.IsActive              AS IsActive,
        u.RegDate               AS RegDate,
        u.ExpiredDate           AS ExpiredDate,
        g.ID                    AS GroupID,
        g.Code                  AS GroupCode,
        g.EngName               AS GroupEnglishName,
        g.ArbName               AS GroupArabicName
    FROM dbo.SystemUsers u
    INNER JOIN dbo.SystemGroups g ON u.GroupID = g.ID
    WHERE u.GroupID = @GroupID
    ORDER BY u.IsActive DESC, u.EngName, u.Code;
END;
