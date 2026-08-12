CREATE OR ALTER PROCEDURE dbo.usp_MasterData_Save
    @EntityKey varchar(50),
    @ID bigint = NULL,
    @Payload nvarchar(max),
    @RegUserID bigint = NULL,
    @SavedID bigint OUTPUT
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF ISJSON(@Payload) <> 1 THROW 50101, 'The supplied record is not valid JSON.', 1;

    DECLARE @TableName sysname, @AllowedColumns nvarchar(max), @AuditUserColumn sysname;
    SELECT @TableName = CASE @EntityKey
        WHEN 'Companies' THEN 'Companies' WHEN 'Branches' THEN 'Branches'
        WHEN 'CostCentersGroup' THEN 'CostCentersGroup' WHEN 'CostCentersTypes' THEN 'CostCentersTypes'
        WHEN 'CostCenters' THEN 'CostCenters' WHEN 'Nationalities' THEN 'Nationalities'
        WHEN 'IdentityTypes' THEN 'IdentityTypes' WHEN 'MaritalStatuses' THEN 'MaritalStatuses'
        WHEN 'Gender' THEN 'Gender' WHEN 'MedicalSpecialties' THEN 'MedicalSpecialties'
        WHEN 'PhysiciansLevels' THEN 'PhysiciansLevels' WHEN 'Physicians' THEN 'Physicians' END,
      @AllowedColumns = CASE @EntityKey
        WHEN 'Companies' THEN 'Code,EngName,ArbName,CRN,VATRN,AddressEngName,AddressArbName,PhoneNo,Email,CountryID,CityID,IsActive'
        WHEN 'Branches' THEN 'CompanyID,Code,EngName,ArbName,CRN,VATRN,AddressEngName,AddressArbName,PhoneNo,Email,CountryID,CityID,IsActive'
        WHEN 'CostCentersGroup' THEN 'Code,EngName,ArbName,Remarks'
        WHEN 'CostCentersTypes' THEN 'Code,EngName,ArbName,Remarks'
        WHEN 'CostCenters' THEN 'CCGID,CCLID,CCTID,Code,EngName,ArbName,Remarks'
        WHEN 'Nationalities' THEN 'Code,Country_EngName,Country_ArbName,Nat_EngName,Nat_ArbName,IsActive'
        WHEN 'IdentityTypes' THEN 'Code,EngName,ArbName,Remarks'
        WHEN 'MaritalStatuses' THEN 'Code,EngName,ArbName,Remarks'
        WHEN 'Gender' THEN 'Code,EngName,ArbName,IsActive'
        WHEN 'MedicalSpecialties' THEN 'Code,EngName,ArbName,Remarks'
        WHEN 'PhysiciansLevels' THEN 'Code,EngName,ArbName,Remarks'
        WHEN 'Physicians' THEN 'CostCenterID,MedicalSpecialtyID,PhysicianLevelID,NationalityID,Code,EngName,ArbName,PrimaryMobile,SecondaryMobile,PrimaryEmail,SecondaryEmail,FollowUpCount,FollowUpPeriod,ConsDuration,FollowUpDuration,IsActive,Remarks' END,
      @AuditUserColumn = CASE WHEN @EntityKey IN ('Companies','Branches') THEN 'RegBy'
                              WHEN @EntityKey IN ('Gender','Nationalities') THEN 'RegUserID'
                              ELSE 'RegUserID' END;

    IF @TableName IS NULL THROW 50100, 'Unsupported master-data entity.', 1;

    DECLARE @Columns table (Ordinal int, ColumnName sysname, TypeExpression nvarchar(128));
    INSERT @Columns
    SELECT s.ordinal, c.name,
           QUOTENAME(TYPE_NAME(c.user_type_id)) + CASE
             WHEN TYPE_NAME(c.user_type_id) IN ('varchar','char','varbinary','binary') THEN '(' + CASE WHEN c.max_length=-1 THEN 'max' ELSE CONVERT(varchar(10),c.max_length) END + ')'
             WHEN TYPE_NAME(c.user_type_id) IN ('nvarchar','nchar') THEN '(' + CASE WHEN c.max_length=-1 THEN 'max' ELSE CONVERT(varchar(10),c.max_length/2) END + ')'
             WHEN TYPE_NAME(c.user_type_id) IN ('decimal','numeric') THEN '(' + CONVERT(varchar(10),c.precision) + ',' + CONVERT(varchar(10),c.scale) + ')'
             ELSE '' END
    FROM STRING_SPLIT(@AllowedColumns, ',', 1) s
    JOIN sys.columns c ON c.object_id=OBJECT_ID(N'dbo.' + @TableName) AND c.name=LTRIM(RTRIM(s.value));

    DECLARE @SetList nvarchar(max), @InsertColumns nvarchar(max), @InsertValues nvarchar(max);
    SELECT @SetList=STRING_AGG(QUOTENAME(ColumnName)+N'=TRY_CONVERT('+TypeExpression+N',JSON_VALUE(@Json,''$.'+ColumnName+N'''))',N',') WITHIN GROUP(ORDER BY Ordinal),
           @InsertColumns=STRING_AGG(QUOTENAME(ColumnName),N',') WITHIN GROUP(ORDER BY Ordinal),
           @InsertValues=STRING_AGG(N'TRY_CONVERT('+TypeExpression+N',JSON_VALUE(@Json,''$.'+ColumnName+N'''))',N',') WITHIN GROUP(ORDER BY Ordinal)
    FROM @Columns;

    DECLARE @Sql nvarchar(max), @HasIdentity bit = COLUMNPROPERTY(OBJECT_ID(N'dbo.'+@TableName),'ID','IsIdentity');
    BEGIN TRY
      BEGIN TRANSACTION;
      IF @ID IS NOT NULL
      BEGIN
        SET @Sql=N'UPDATE dbo.'+QUOTENAME(@TableName)+N' SET '+@SetList+N' WHERE ID=@RecordID; IF @@ROWCOUNT=0 THROW 50102,''Record not found.'',1; SET @OutID=@RecordID;';
      END
      ELSE IF @HasIdentity=1
      BEGIN
        SET @Sql=N'INSERT dbo.'+QUOTENAME(@TableName)+N' ('+@InsertColumns+
          CASE WHEN COL_LENGTH('dbo.'+@TableName,@AuditUserColumn) IS NOT NULL THEN N','+QUOTENAME(@AuditUserColumn) ELSE N'' END+
          N') VALUES ('+@InsertValues+CASE WHEN COL_LENGTH('dbo.'+@TableName,@AuditUserColumn) IS NOT NULL THEN N',@ActorID' ELSE N'' END+N'); SET @OutID=CONVERT(bigint,SCOPE_IDENTITY());';
      END
      ELSE
      BEGIN
        SET @Sql=N'SELECT @OutID=ISNULL(MAX(ID),0)+1 FROM dbo.'+QUOTENAME(@TableName)+N' WITH (UPDLOCK,HOLDLOCK); INSERT dbo.'+QUOTENAME(@TableName)+N' (ID,'+@InsertColumns+
          CASE WHEN COL_LENGTH('dbo.'+@TableName,@AuditUserColumn) IS NOT NULL THEN N','+QUOTENAME(@AuditUserColumn) ELSE N'' END+
          N') VALUES (@OutID,'+@InsertValues+CASE WHEN COL_LENGTH('dbo.'+@TableName,@AuditUserColumn) IS NOT NULL THEN N',@ActorID' ELSE N'' END+N');';
      END;
      EXEC sys.sp_executesql @Sql,N'@Json nvarchar(max),@RecordID bigint,@ActorID bigint,@OutID bigint OUTPUT',@Json=@Payload,@RecordID=@ID,@ActorID=@RegUserID,@OutID=@SavedID OUTPUT;
      COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
      IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
      THROW;
    END CATCH;
END;
