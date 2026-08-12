USE [ARBP];
GO
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.Companies', N'LocationCountryID') IS NOT NULL
   AND COL_LENGTH(N'dbo.Companies', N'CountryID') IS NULL
    EXEC sys.sp_rename N'dbo.Companies.LocationCountryID', N'CountryID', N'COLUMN';

IF COL_LENGTH(N'dbo.Companies', N'LocationCityID') IS NOT NULL
   AND COL_LENGTH(N'dbo.Companies', N'CityID') IS NULL
    EXEC sys.sp_rename N'dbo.Companies.LocationCityID', N'CityID', N'COLUMN';

IF COL_LENGTH(N'dbo.Branches', N'LocationCountryID') IS NOT NULL
   AND COL_LENGTH(N'dbo.Branches', N'CountryID') IS NULL
    EXEC sys.sp_rename N'dbo.Branches.LocationCountryID', N'CountryID', N'COLUMN';

IF COL_LENGTH(N'dbo.Branches', N'LocationCityID') IS NOT NULL
   AND COL_LENGTH(N'dbo.Branches', N'CityID') IS NULL
    EXEC sys.sp_rename N'dbo.Branches.LocationCityID', N'CityID', N'COLUMN';

IF COL_LENGTH(N'dbo.Companies', N'CountryID') IS NULL OR COL_LENGTH(N'dbo.Companies', N'CityID') IS NULL
   OR COL_LENGTH(N'dbo.Branches', N'CountryID') IS NULL OR COL_LENGTH(N'dbo.Branches', N'CityID') IS NULL
    THROW 50950, 'Location column rename did not complete.', 1;

COMMIT TRANSACTION;
GO
