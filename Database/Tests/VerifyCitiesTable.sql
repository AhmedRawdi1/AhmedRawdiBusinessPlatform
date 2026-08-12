USE [ARBP];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Cities', N'U') IS NULL THROW 50960, 'Cities table does not exist.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.Cities') AND name=N'FK_Cities_Nationalities_CountryID')
    THROW 50961, 'Cities country relationship is missing.', 1;
IF (SELECT COUNT(*) FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.Cities') AND referenced_object_id=OBJECT_ID(N'dbo.SystemUsers')) <> 2
    THROW 50962, 'Cities user audit relationships are missing.', 1;

BEGIN TRANSACTION;
DECLARE @CountryID bigint=(SELECT TOP(1) ID FROM dbo.Nationalities ORDER BY ID);
DECLARE @UserID bigint=(SELECT TOP(1) ID FROM dbo.SystemUsers WHERE IsActive=1 AND CancellationDate IS NULL ORDER BY IsSystemOwner DESC,ID);
DECLARE @CityID bigint=NEXT VALUE FOR dbo.Seq_CitiesID;
INSERT dbo.Cities(ID,CountryID,Code,EngName,ArabName,RegUserID)
VALUES(@CityID,@CountryID,N'CITY-TEST',N'City Test',N'اختبار مدينة',@UserID);
IF NOT EXISTS(SELECT 1 FROM dbo.Cities WHERE ID=@CityID AND CountryID=@CountryID AND CancelDate IS NULL)
    THROW 50963, 'Cities insert verification failed.', 1;
ROLLBACK TRANSACTION;

IF EXISTS(SELECT 1 FROM dbo.Cities WHERE Code=N'CITY-TEST') THROW 50964, 'Cities test rollback failed.', 1;
SELECT N'CitiesTable' AS Test,CONVERT(bit,1) AS Passed;
GO
