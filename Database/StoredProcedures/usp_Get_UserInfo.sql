USE [ARBP];
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_Get_UserInfo]
    @UserCode nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @UserCode = NULLIF(TRIM(@UserCode), N'');

    IF @UserCode IS NULL
        THROW 50020, 'UserCode is required.', 1;

    SELECT
        su.ID                                   AS UserID,
        su.Code                                 AS UserCode,
        su.EngName                              AS UserEnglishName,
        su.ArbName                              AS UserArabicName,
        CONVERT(NVARCHAR(256), DECRYPTBYPASSPHRASE(N'ARBP_Secure_Passphrase_Key_2026', su.UserPass)) AS UserPass,
        su.Email,
        su.MobileNum,
        su.IsActive,
        su.ExpiredDate,
        su.PreferredLanguage,
        sg.ID                                    AS GroupID,
        sg.Code                                  AS GroupCode,
        sg.EngName                               AS GroupEnglishName,
        sg.ArbName                               AS GroupArabicName
    FROM dbo.SystemUsers AS su
    INNER JOIN dbo.SystemGroups AS sg
        ON sg.ID = su.GroupID
    WHERE su.Code = @UserCode
      AND su.CancellationDate IS NULL;
END;
GO
