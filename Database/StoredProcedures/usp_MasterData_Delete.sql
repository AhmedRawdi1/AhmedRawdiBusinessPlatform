CREATE OR ALTER PROCEDURE dbo.usp_MasterData_Delete
    @EntityKey varchar(50), @ID bigint, @CancelUserID bigint = NULL
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @TableName sysname = CASE @EntityKey
      WHEN 'Companies' THEN 'Companies' WHEN 'Branches' THEN 'Branches' WHEN 'CostCentersGroup' THEN 'CostCentersGroup'
      WHEN 'CostCentersTypes' THEN 'CostCentersTypes' WHEN 'CostCenters' THEN 'CostCenters' WHEN 'Nationalities' THEN 'Nationalities'
      WHEN 'IdentityTypes' THEN 'IdentityTypes' WHEN 'MaritalStatuses' THEN 'MaritalStatuses' WHEN 'Gender' THEN 'Gender'
      WHEN 'MedicalSpecialties' THEN 'MedicalSpecialties' WHEN 'PhysiciansLevels' THEN 'PhysiciansLevels' WHEN 'Physicians' THEN 'Physicians' END;
    IF @TableName IS NULL THROW 50100, 'Unsupported master-data entity.', 1;
    DECLARE @DateColumn sysname=CASE WHEN @EntityKey IN('Companies','Branches') THEN 'CancellationDate' ELSE 'CancelDate' END;
    DECLARE @UserColumn sysname=CASE WHEN @EntityKey IN('Companies','Branches') THEN 'CancelBy' ELSE 'CancelUserID' END;
    DECLARE @Sql nvarchar(max)=N'UPDATE dbo.'+QUOTENAME(@TableName)+N' SET '+
      CASE WHEN COL_LENGTH('dbo.'+@TableName,'IsActive') IS NOT NULL THEN N'IsActive=0,' ELSE N'' END+
      QUOTENAME(@DateColumn)+N'=SYSUTCDATETIME(),'+QUOTENAME(@UserColumn)+N'=@ActorID WHERE ID=@RecordID; IF @@ROWCOUNT=0 THROW 50102,''Record not found.'',1;';
    EXEC sys.sp_executesql @Sql,N'@RecordID bigint,@ActorID bigint',@RecordID=@ID,@ActorID=@CancelUserID;
END;
