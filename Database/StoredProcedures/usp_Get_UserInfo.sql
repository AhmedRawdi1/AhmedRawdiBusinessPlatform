USE [ARBP];
GO

CREATE OR ALTER PROCEDURE dbo.usp_Get_UserInfo
    @UserCode nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;

    SET @UserCode = NULLIF(LTRIM(RTRIM(@UserCode)), N'');
    IF @UserCode IS NULL THROW 50020, 'User code is required.', 1;

    SELECT
        su.ID AS UserID,
        su.Code AS UserCode,
        su.EngName AS UserEnglishName,
        su.ArbName AS UserArabicName,
        su.PasswordHash,
        su.IsSystemOwner,
        su.MustChangePassword,
        su.SecurityStamp,
        su.Email,
        su.MobileNum,
        su.IsActive,
        su.ExpiredDate,
        su.PreferredLanguage,
        sg.ID AS GroupID,
        sg.Code AS GroupCode,
        sg.EngName AS GroupEnglishName,
        sg.ArbName AS GroupArabicName
    FROM dbo.SystemUsers su
    INNER JOIN dbo.SystemGroups sg ON sg.ID = su.GroupID
    WHERE su.Code = @UserCode
      AND su.CancellationDate IS NULL;
END;
GO
