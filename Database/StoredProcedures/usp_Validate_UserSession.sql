USE [ARBP];
GO

CREATE OR ALTER PROCEDURE dbo.usp_Validate_UserSession
    @UserID bigint,
    @SecurityStamp uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CONVERT(bit, CASE WHEN EXISTS
    (
        SELECT 1 FROM dbo.SystemUsers
        WHERE ID=@UserID AND SecurityStamp=@SecurityStamp AND IsActive=1 AND CancellationDate IS NULL
          AND (ExpiredDate IS NULL OR ExpiredDate>=GETDATE())
    ) THEN 1 ELSE 0 END) AS IsValid;
END;
GO
