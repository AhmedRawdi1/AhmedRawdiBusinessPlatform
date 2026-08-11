SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Delete_User
    @UserID bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @UserID IS NULL
        THROW 50001, 'A valid user ID is required.', 1;

    IF EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE ID=@UserID AND IsSystemOwner=1)
        THROW 50230, 'The system owner account cannot be deactivated.', 1;

    UPDATE dbo.SystemUsers
    SET IsActive = 0
    WHERE ID = @UserID;

    IF @@ROWCOUNT = 0
        THROW 50002, 'The specified user does not exist.', 1;
END;
