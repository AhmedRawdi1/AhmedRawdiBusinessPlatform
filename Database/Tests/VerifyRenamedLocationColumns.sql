USE [ARBP];
GO
SET NOCOUNT ON;

IF EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id IN (OBJECT_ID(N'dbo.Companies'),OBJECT_ID(N'dbo.Branches'))
      AND name IN(N'LocationCountryID',N'LocationCityID')
)
    THROW 50951, 'Legacy location columns still exist.', 1;

IF (SELECT COUNT(*) FROM sys.columns WHERE object_id IN(OBJECT_ID(N'dbo.Companies'),OBJECT_ID(N'dbo.Branches')) AND name IN(N'CountryID',N'CityID')) <> 4
    THROW 50952, 'CountryID or CityID is missing.', 1;

SELECT N'RenamedLocationColumns' AS Test,CONVERT(bit,1) AS Passed;
GO
