USE [ARBP];
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_Add_SystemGroups]
    @Code nvarchar(50),
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
            SET @ArbName = @EngName;

        BEGIN TRANSACTION;

        IF ISNULL(@GroupID, 0) = 0
        BEGIN
            DECLARE @NextID BIGINT = ISNULL((SELECT MAX(ID) FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK)), 0) + 1;
            DECLARE @IdStr NVARCHAR(20) = CAST(@NextID AS NVARCHAR(20));
            DECLARE @FinalCode NVARCHAR(20) = @Code;

            IF @Code NOT LIKE '%' + @IdStr
            BEGIN
                SET @FinalCode = SUBSTRING(@Code + '_' + @IdStr, 1, 20);
            END;

            IF EXISTS (SELECT 1 FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK) WHERE Code = @FinalCode)
                THROW 50004, 'A system group with the same code already exists.', 1;

            INSERT dbo.SystemGroups
                (ID, Code, EngName, ArbName, IsActive, RegBy, ExpiredDate, RegDate)
            VALUES
                (@NextID, @FinalCode, @EngName, @ArbName, @IsActive, @RegBy, @ExpiredDate, GETDATE());

            SET @NewGroupID = @NextID;
        END
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK) WHERE ID = @GroupID)
                THROW 50005, 'The system group to update was not found.', 1;

            SET @NewGroupID = @GroupID;
            DECLARE @UpdateIdStr NVARCHAR(20) = CAST(@GroupID AS NVARCHAR(20));
            DECLARE @UpdatedCode NVARCHAR(20) = @Code;

            IF @Code NOT LIKE '%' + @UpdateIdStr
            BEGIN
                SET @UpdatedCode = SUBSTRING(@Code + '_' + @UpdateIdStr, 1, 20);
            END;

            IF EXISTS (SELECT 1 FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK) WHERE Code = @UpdatedCode AND ID <> @GroupID)
                THROW 50004, 'A system group with the same code already exists.', 1;

            UPDATE dbo.SystemGroups
            SET Code = @UpdatedCode,
                EngName = @EngName,
                ArbName = @ArbName,
                IsActive = @IsActive,
                RegBy = COALESCE(@RegBy, RegBy),
                ExpiredDate = @ExpiredDate
            WHERE ID = @GroupID;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SET @HasError = 1;
        SET @ErrorDesc = ERROR_MESSAGE();

        THROW;
    END CATCH;
END;
GO
