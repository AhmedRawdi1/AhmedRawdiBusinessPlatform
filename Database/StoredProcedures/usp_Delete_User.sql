CREATE OR ALTER PROCEDURE dbo.usp_Delete_User
    @UserID bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @UserID IS NULL
        THROW 50001, 'A valid user ID is required.', 1;

    UPDATE dbo.SystemUsers
    SET IsActive = 0
    WHERE ID = @UserID;

    IF @@ROWCOUNT = 0
        THROW 50002, 'The specified user does not exist.', 1;
END;
