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
    @NewUserID bigint = NULL OUTPUT,
    @UserPass nvarchar(256) = NULL,
    @Passphrase nvarchar(128) = N'ARBP_Secure_Passphrase_Key_2026'
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
        SET @UserPass = NULLIF(LTRIM(RTRIM(@UserPass)), N'');
        SET @PreferredLanguage = COALESCE(NULLIF(LTRIM(RTRIM(@PreferredLanguage)), ''), 'en-US');

        IF @Code IS NULL THROW 50051, 'User code is required.', 1;
        IF @EngName IS NULL THROW 50052, 'User English name is required.', 1;

        IF @ArbName IS NULL SET @ArbName = @EngName;

        DECLARE @EncryptedPass VARBINARY(MAX) = NULL;
        IF @UserPass IS NOT NULL
        BEGIN
            SET @EncryptedPass = ENCRYPTBYPASSPHRASE(@Passphrase, @UserPass);
        END;

        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.SystemGroups WITH (UPDLOCK, HOLDLOCK) WHERE ID = @GroupID AND IsActive = 1 AND CancellationDate IS NULL)
            THROW 50050, 'The supplied group is not active or does not exist.', 1;

        IF ISNULL(@UserID, 0) = 0
        BEGIN
            DECLARE @NextID BIGINT = ISNULL((SELECT MAX(ID) FROM dbo.SystemUsers WITH (UPDLOCK, HOLDLOCK)), 0) + 1;
            DECLARE @IdStr NVARCHAR(20) = CAST(@NextID AS NVARCHAR(20));
            DECLARE @FinalCode NVARCHAR(100) = @Code;

            IF @Code NOT LIKE '%' + @IdStr
            BEGIN
                SET @FinalCode = SUBSTRING(@Code + '_' + @IdStr, 1, 100);
            END;

            IF EXISTS (SELECT 1 FROM dbo.SystemUsers WITH (UPDLOCK, HOLDLOCK) WHERE Code = @FinalCode)
                THROW 50053, 'A system user with the same code already exists.', 1;

            INSERT dbo.SystemUsers
                (ID, GroupID, Code, EngName, ArbName, IsActive, UserPass, Email, MobileNum, PreferredLanguage, RegBy, ExpiredDate, RegDate)
            VALUES
                (@NextID, @GroupID, @FinalCode, @EngName, @ArbName, @IsActive, @EncryptedPass, @Email, @MobileNum, @PreferredLanguage, @RegBy, @ExpiredDate, GETDATE());

            SET @NewUserID = @NextID;
        END
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WITH (UPDLOCK, HOLDLOCK) WHERE ID = @UserID)
                THROW 50054, 'The system user to update was not found.', 1;

            SET @NewUserID = @UserID;
            DECLARE @UpdateIdStr NVARCHAR(20) = CAST(@UserID AS NVARCHAR(20));
            DECLARE @UpdatedCode NVARCHAR(100) = @Code;

            IF @Code NOT LIKE '%' + @UpdateIdStr
            BEGIN
                SET @UpdatedCode = SUBSTRING(@Code + '_' + @UpdateIdStr, 1, 100);
            END;

            IF EXISTS (SELECT 1 FROM dbo.SystemUsers WITH (UPDLOCK, HOLDLOCK) WHERE Code = @UpdatedCode AND ID <> @UserID)
                THROW 50053, 'A system user with the same code already exists.', 1;

            UPDATE dbo.SystemUsers
            SET GroupID = @GroupID,
                Code = @UpdatedCode,
                EngName = @EngName,
                ArbName = @ArbName,
                IsActive = @IsActive,
                UserPass = COALESCE(@EncryptedPass, UserPass),
                Email = @Email,
                MobileNum = @MobileNum,
                PreferredLanguage = @PreferredLanguage,
                RegBy = COALESCE(@RegBy, RegBy),
                ExpiredDate = @ExpiredDate
            WHERE ID = @UserID;
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
