USE [ARBP];
GO
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.Companies', N'LocationCityID') IS NOT NULL
   AND EXISTS
   (
       SELECT 1 FROM sys.columns
       WHERE object_id=OBJECT_ID(N'dbo.Companies')
         AND name=N'LocationCityID'
         AND is_nullable=0
   )
BEGIN
    ALTER TABLE dbo.Companies ALTER COLUMN LocationCityID bigint NULL;
END;
GO
