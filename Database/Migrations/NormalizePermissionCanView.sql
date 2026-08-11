SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

UPDATE dbo.SystemFormsPermissions
SET CanView = 1
WHERE CancelledDate IS NULL
  AND CanView = 0
  AND (CanSave = 1 OR CanUpdate = 1 OR CanDelete = 1 OR CanSearch = 1 OR CanPrint = 1);

IF EXISTS
(
    SELECT 1 FROM dbo.SystemFormsPermissions
    WHERE CancelledDate IS NULL AND CanView=0
      AND (CanSave=1 OR CanUpdate=1 OR CanDelete=1 OR CanSearch=1 OR CanPrint=1)
)
    THROW 50006, 'Permission normalization did not complete.', 1;

COMMIT TRANSACTION;
