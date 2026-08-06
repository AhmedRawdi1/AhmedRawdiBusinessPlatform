USE [ARBP];
GO

CREATE OR ALTER PROCEDURE dbo.usp_Add_SystemUser
    @GroupID bigint,
    @Code nvarchar(100),
    @EngName nvarchar(150),
    @ArbName nvarchar(150),
    @IsActive bit,
    @Email nvarchar(200),
    @MobileNum nvarchar(50) = NULL,
    @RegBy bigint = NULL,
    @ExpiredDate smalldatetime = NULL,
    @HasError bit OUTPUT,
    @ErrorDesc nvarchar(2048) OUTPUT,
    @UserID bigint = NULL,
    @PreferredLanguage varchar(25) = NULL,
    @NewUserID bigint = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @HasError = 0;
    SET @ErrorDesc = NULL;
    SET @NewUserID = NULL;

    BEGIN TRY
        SET @Code = NULLIF(LTRIM(RTRIM(@Code)), N'');
        SET @EngName = NULLIF(LTRIM(RTRIM(@EngName)), N'');
        SET @ArbName = NULLIF(LTRIM(RTRIM(@ArbName)), N'');
        SET @Email = NULLIF(LTRIM(RTRIM(@Email)), N'');
        SET @MobileNum = NULLIF(LTRIM(RTRIM(@MobileNum)), N'');
        SET @PreferredLanguage = COALESCE(NULLIF(LTRIM(RTRIM(@PreferredLanguage)), ''), 'en-US');

        IF @Code IS NULL THROW 50051, 'User code is required.', 1;
        IF @EngName IS NULL THROW 50052, 'User English name is required.', 1;

        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.SystemGroups WHERE ID = @GroupID AND IsActive = 1 AND CancellationDate IS NULL)
            THROW 50050, 'The supplied group is not active or does not exist.', 1;

        IF EXISTS (SELECT 1 FROM dbo.SystemUsers WITH (UPDLOCK, HOLDLOCK) WHERE Code = @Code AND ID <> ISNULL(@UserID, 0))
            THROW 50053, 'A system user with the same code already exists.', 1;

        IF ISNULL(@UserID, 0) = 0
        BEGIN
            INSERT dbo.SystemUsers
                (GroupID, Code, EngName, ArbName, IsActive, UserPass, Email, MobileNum, PreferredLanguage, RegBy, ExpiredDate)
            VALUES
                (@GroupID, @Code, @EngName, @ArbName, @IsActive, NULL, @Email, @MobileNum, @PreferredLanguage, @RegBy, @ExpiredDate);

            SET @NewUserID = CONVERT(bigint, SCOPE_IDENTITY());
        END
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WITH (UPDLOCK, HOLDLOCK) WHERE ID = @UserID)
                THROW 50054, 'The system user to update was not found.', 1;

            UPDATE dbo.SystemUsers
            SET GroupID = @GroupID,
                Code = @Code,
                EngName = @EngName,
                ArbName = @ArbName,
                IsActive = @IsActive,
                Email = @Email,
                MobileNum = @MobileNum,
                PreferredLanguage = @PreferredLanguage,
                RegBy = COALESCE(@RegBy, RegBy),
                ExpiredDate = @ExpiredDate
            WHERE ID = @UserID;

            SET @NewUserID = @UserID;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        SET @HasError = 1;
        SET @ErrorDesc = ERROR_MESSAGE();
        THROW;
    END CATCH;
END;
GO
