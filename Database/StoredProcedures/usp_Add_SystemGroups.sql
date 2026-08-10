USE [ARBP];
GO

CREATE OR ALTER PROCEDURE dbo.usp_Add_SystemGroups
    @Code nvarchar(20),
    @EngName nvarchar(150),
    @ArbName nvarchar(150) = NULL,
    @IsActive bit,
    @RegBy bigint = NULL,
    @ExpiredDate smalldatetime = NULL,
    @NewGroupID bigint OUTPUT,
    @HasError bit OUTPUT,
    @ErrorDesc nvarchar(2048) OUTPUT,
    @GroupID bigint = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT @NewGroupID = NULL, @HasError = 0, @ErrorDesc = NULL;

    BEGIN TRY
        SELECT
            @Code = NULLIF(LTRIM(RTRIM(@Code)), N''),
            @EngName = NULLIF(LTRIM(RTRIM(@EngName)), N''),
            @ArbName = NULLIF(LTRIM(RTRIM(@ArbName)), N'');

        IF @Code IS NULL THROW 50001, 'Group code is required.', 1;
        IF @EngName IS NULL THROW 50002, 'Group English name is required.', 1;
        SET @ArbName = COALESCE(@ArbName, @EngName);

        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK) WHERE Code = @Code AND ID <> ISNULL(@GroupID, 0))
            THROW 50004, 'A system group with the same code already exists.', 1;

        IF ISNULL(@GroupID, 0) = 0
        BEGIN
            SET @NewGroupID = NEXT VALUE FOR dbo.Seq_SystemGroupID;

            INSERT dbo.SystemGroups (ID, Code, EngName, ArbName, IsActive, RegBy, ExpiredDate)
            VALUES (@NewGroupID, @Code, @EngName, @ArbName, @IsActive, @RegBy, @ExpiredDate);
        END
        ELSE
        BEGIN
            UPDATE dbo.SystemGroups
            SET Code = @Code,
                EngName = @EngName,
                ArbName = @ArbName,
                IsActive = @IsActive,
                RegBy = COALESCE(@RegBy, RegBy),
                ExpiredDate = @ExpiredDate,
                CancellationDate = CASE WHEN @IsActive = 1 THEN NULL ELSE CancellationDate END
            WHERE ID = @GroupID;

            IF @@ROWCOUNT = 0 THROW 50005, 'The system group to update was not found.', 1;
            SET @NewGroupID = @GroupID;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        SELECT @HasError = 1, @ErrorDesc = ERROR_MESSAGE();
        THROW;
    END CATCH;
END;
GO
