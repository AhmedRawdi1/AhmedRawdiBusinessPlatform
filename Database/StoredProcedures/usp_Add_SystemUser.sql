USE [ARBP];
GO

CREATE OR ALTER PROCEDURE dbo.usp_Add_SystemUser
    @GroupID bigint,
    @Code nvarchar(100),
    @EngName nvarchar(150),
    @ArbName nvarchar(150) = NULL,
    @IsActive bit,
    @Email nvarchar(200) = NULL,
    @MobileNum nvarchar(50) = NULL,
    @RegBy bigint = NULL,
    @ExpiredDate smalldatetime = NULL,
    @HasError bit OUTPUT,
    @ErrorDesc nvarchar(2048) OUTPUT,
    @UserID bigint = NULL,
    @PreferredLanguage varchar(25) = 'en-US',
    @NewUserID bigint = NULL OUTPUT,
    @PasswordHash nvarchar(512) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT @HasError = 0, @ErrorDesc = NULL, @NewUserID = NULL;

    BEGIN TRY
        SELECT
            @Code = NULLIF(LTRIM(RTRIM(@Code)), N''),
            @EngName = NULLIF(LTRIM(RTRIM(@EngName)), N''),
            @ArbName = NULLIF(LTRIM(RTRIM(@ArbName)), N''),
            @Email = NULLIF(LTRIM(RTRIM(@Email)), N''),
            @MobileNum = NULLIF(LTRIM(RTRIM(@MobileNum)), N''),
            @PasswordHash = NULLIF(LTRIM(RTRIM(@PasswordHash)), N''),
            @PreferredLanguage = COALESCE(NULLIF(LTRIM(RTRIM(@PreferredLanguage)), ''), 'en-US');

        IF @Code IS NULL THROW 50051, 'User code is required.', 1;
        IF @EngName IS NULL THROW 50052, 'User English name is required.', 1;
        IF @PreferredLanguage NOT IN ('en-US', 'ar-SA') THROW 50055, 'Preferred language is invalid.', 1;
        IF ISNULL(@UserID, 0) = 0 AND @PasswordHash IS NULL THROW 50056, 'A password hash is required for a new user.', 1;
        SET @ArbName = COALESCE(@ArbName, @EngName);

        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK) WHERE ID = @GroupID AND IsActive = 1 AND CancellationDate IS NULL)
            THROW 50050, 'The supplied group is not active or does not exist.', 1;

        IF EXISTS (SELECT 1 FROM dbo.SystemUsers WITH (UPDLOCK, HOLDLOCK) WHERE Code = @Code AND ID <> ISNULL(@UserID, 0))
            THROW 50053, 'A system user with the same code already exists.', 1;

        IF ISNULL(@UserID, 0) = 0
        BEGIN
            SET @NewUserID = NEXT VALUE FOR dbo.Seq_SystemUserID;

            INSERT dbo.SystemUsers
                (ID, GroupID, Code, EngName, ArbName, IsActive, PasswordHash, PasswordChangedAt, Email, MobileNum, PreferredLanguage, RegBy, ExpiredDate)
            VALUES
                (@NewUserID, @GroupID, @Code, @EngName, @ArbName, @IsActive, @PasswordHash, SYSUTCDATETIME(), @Email, @MobileNum, @PreferredLanguage, @RegBy, @ExpiredDate);
        END
        ELSE
        BEGIN
            UPDATE dbo.SystemUsers
            SET GroupID = @GroupID,
                Code = @Code,
                EngName = @EngName,
                ArbName = @ArbName,
                IsActive = @IsActive,
                PasswordHash = COALESCE(@PasswordHash, PasswordHash),
                PasswordChangedAt = CASE WHEN @PasswordHash IS NULL THEN PasswordChangedAt ELSE SYSUTCDATETIME() END,
                Email = @Email,
                MobileNum = @MobileNum,
                PreferredLanguage = @PreferredLanguage,
                RegBy = COALESCE(@RegBy, RegBy),
                ExpiredDate = @ExpiredDate,
                CancellationDate = CASE WHEN @IsActive = 1 THEN NULL ELSE CancellationDate END
            WHERE ID = @UserID;

            IF @@ROWCOUNT = 0 THROW 50054, 'The system user to update was not found.', 1;
            SET @NewUserID = @UserID;
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
