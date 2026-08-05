USE [ARBP];
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_Add_SystemGroups]
    @Code nvarchar(20),
    @EngName nvarchar(150),
    @ArbName nvarchar(150),
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

    SET @NewGroupID = NULL;
    SET @HasError = 0;
    SET @ErrorDesc = NULL;

    BEGIN TRY
        SET @Code = NULLIF(LTRIM(RTRIM(@Code)), N'');
        SET @EngName = NULLIF(LTRIM(RTRIM(@EngName)), N'');
        SET @ArbName = NULLIF(LTRIM(RTRIM(@ArbName)), N'');

        IF @Code IS NULL
            THROW 50001, 'Group code is required.', 1;

        IF @EngName IS NULL
            THROW 50002, 'Group English name is required.', 1;

        IF @ArbName IS NULL
            THROW 50003, 'Group Arabic name is required.', 1;

        BEGIN TRANSACTION;

        IF ISNULL(@GroupID, 0) = 0
        BEGIN
            IF EXISTS (SELECT 1 FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK) WHERE Code = @Code)
                THROW 50004, 'A system group with the same code already exists.', 1;

            INSERT dbo.SystemGroups
                (Code, EngName, ArbName, IsActive, RegBy, ExpiredDate)
            VALUES
                (@Code, @EngName, @ArbName, @IsActive, @RegBy, @ExpiredDate);

            SET @NewGroupID = CONVERT(bigint, SCOPE_IDENTITY());
        END
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK) WHERE ID = @GroupID)
                THROW 50005, 'The system group to update was not found.', 1;

            IF EXISTS (SELECT 1 FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK) WHERE Code = @Code AND ID <> @GroupID)
                THROW 50004, 'A system group with the same code already exists.', 1;

            UPDATE dbo.SystemGroups
            SET Code = @Code,
                EngName = @EngName,
                ArbName = @ArbName,
                IsActive = @IsActive,
                RegBy = COALESCE(@RegBy, RegBy),
                ExpiredDate = @ExpiredDate
            WHERE ID = @GroupID;

            SET @NewGroupID = @GroupID;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SET @HasError = 1;
        SET @ErrorDesc = ERROR_MESSAGE();

        -- Preserve the existing procedure behavior for callers that handle SQL exceptions.
        THROW;
    END CATCH;
END;
GO
