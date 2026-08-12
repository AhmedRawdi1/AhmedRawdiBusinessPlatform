USE [ARBP];
GO

SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Cities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cities
    (
        ID bigint NOT NULL,
        CountryID bigint NOT NULL,
        Code nvarchar(50) NOT NULL,
        EngName nvarchar(200) NOT NULL,
        ArabName nvarchar(200) NOT NULL,
        RegDate datetime2(0) NOT NULL CONSTRAINT DF_Cities_RegDate DEFAULT (SYSUTCDATETIME()),
        RegUserID bigint NOT NULL,
        CancelDate datetime2(0) NULL,
        CancelUserID bigint NULL,

        CONSTRAINT PK_Cities PRIMARY KEY CLUSTERED (ID),
        CONSTRAINT FK_Cities_Nationalities_CountryID FOREIGN KEY (CountryID) REFERENCES dbo.Nationalities(ID),
        CONSTRAINT FK_Cities_SystemUsers_RegUserID FOREIGN KEY (RegUserID) REFERENCES dbo.SystemUsers(ID),
        CONSTRAINT FK_Cities_SystemUsers_CancelUserID FOREIGN KEY (CancelUserID) REFERENCES dbo.SystemUsers(ID),
        CONSTRAINT CK_Cities_Code_NotBlank CHECK (LEN(LTRIM(RTRIM(Code))) > 0),
        CONSTRAINT CK_Cities_EngName_NotBlank CHECK (LEN(LTRIM(RTRIM(EngName))) > 0),
        CONSTRAINT CK_Cities_ArabName_NotBlank CHECK (LEN(LTRIM(RTRIM(ArabName))) > 0),
        CONSTRAINT CK_Cities_Cancellation CHECK
        (
            (CancelDate IS NULL AND CancelUserID IS NULL)
            OR (CancelDate IS NOT NULL AND CancelUserID IS NOT NULL)
        )
    );

    CREATE UNIQUE INDEX UX_Cities_CountryID_Code
        ON dbo.Cities(CountryID, Code)
        WHERE CancelDate IS NULL;

    CREATE INDEX IX_Cities_CountryID_Active
        ON dbo.Cities(CountryID, CancelDate)
        INCLUDE (Code, EngName, ArabName);
END;

IF OBJECT_ID(N'dbo.Seq_CitiesID', N'SO') IS NULL
    CREATE SEQUENCE dbo.Seq_CitiesID AS bigint START WITH 1 INCREMENT BY 1;

COMMIT TRANSACTION;
GO
