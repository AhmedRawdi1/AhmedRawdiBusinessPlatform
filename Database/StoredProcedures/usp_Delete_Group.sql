CREATE OR ALTER PROCEDURE dbo.usp_Delete_Group
    @GroupID bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @GroupID IS NULL
        THROW 50001, 'A valid group ID is required.', 1;

    UPDATE dbo.SystemGroups
    SET IsActive = 0
    WHERE ID = @GroupID;

    IF @@ROWCOUNT = 0
        THROW 50002, 'The specified group does not exist.', 1;
END;
