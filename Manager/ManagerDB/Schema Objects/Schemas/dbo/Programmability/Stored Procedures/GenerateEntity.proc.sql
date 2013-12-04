CREATE PROCEDURE [dbo].[GenerateEntity]
  @tableName VARCHAR(60),
  @getset BIT = 0
AS
BEGIN
	DECLARE @output AS VARCHAR(MAX);
	DECLARE @newline AS VARCHAR(2) =  CHAR(13) + CHAR(10)
	DECLARE @lines AS TABLE ( line VARCHAR(255) )
	DECLARE @line VARCHAR(255)
	
	SET NOCOUNT ON
	
	-- Generate entity class
	INSERT INTO @lines
	SELECT 'public ' +
		CASE WHEN DATA_TYPE='varchar' OR DATA_TYPE='nvarchar' THEN 'string'
		     WHEN DATA_TYPE='tinyint' THEN 'byte'  
			 WHEN DATA_TYPE='bit' THEN 'bool' 
             WHEN DATA_TYPE='datetime' OR DATA_TYPE='datetime2' THEN 'DateTime' 
			 ELSE DATA_TYPE END + 
		CASE WHEN IS_NULLABLE='YES' AND NOT (DATA_TYPE='varchar' OR DATA_TYPE='nvarchar') THEN '? ' ELSE ' ' END + COLUMN_NAME +
		CASE WHEN @getset = 0 THEN ';' ELSE ' { get; set; }' END
	FROM  INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName
	
	SET @output = 'public class ' + @tableName + @newline + '{' + @newline;
	
	DECLARE cur CURSOR FOR SELECT line FROM @lines
	OPEN cur 
	FETCH cur INTO @line
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @output += '    ' + @line + @newline
		FETCH cur INTO @line	
	END
	CLOSE cur
    DEALLOCATE cur
    
    --SELECT @output + '}'  -- 在text output 窗口不能完整输出
    SET @output += '}' + @newline 
	DELETE @lines
	
	-- Generate statements for load date from DataReader.
	DECLARE @variableName VARCHAR(255) = LOWER(LEFT(@tableName, 1)) + RIGHT(@tableName, LEN(@tableName)-1)  -- 变量名首字母小写
    INSERT INTO @lines
    SELECT @variableName + '.' + COLUMN_NAME + ' = '+
        CASE WHEN IS_NULLABLE='YES' THEN 'reader["'+COLUMN_NAME+'"] == DBNull.Value ? null : ' ELSE '' END +'(' + 
		CASE WHEN DATA_TYPE='varchar' OR DATA_TYPE='nvarchar' THEN 'string'
		     WHEN DATA_TYPE='tinyint' THEN 'byte'  
			 WHEN DATA_TYPE='bit' THEN 'bool' 
			 WHEN DATA_TYPE='datetime' OR DATA_TYPE='datetime2' THEN 'DateTime' 
			 ELSE DATA_TYPE END + 
	   CASE WHEN IS_NULLABLE='YES' AND NOT (DATA_TYPE='varchar' OR DATA_TYPE='nvarchar') THEN '?' ELSE '' END + ')reader["' + COLUMN_NAME + '"];'
    FROM INFORMATION_SCHEMA.[COLUMNS] WHERE TABLE_NAME = @tableName
    
    SET @output += @tableName + ' ' + @variableName + ' = new ' + @tableName + '();' + @newline;

	DECLARE cur CURSOR FOR SELECT line FROM @lines
	OPEN cur 
	FETCH cur INTO @line
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @output += @line + @newline
		FETCH cur INTO @line	
	END
	CLOSE cur
    DEALLOCATE cur
    
    -- Generate statements for insert objects
    SET @output += @newline + 'public static int AddMetadataObject('+@tableName+' entity)' + @newline + '{'
    
    DECLARE @columns VARCHAR(MAX) = '    string sql = "INSERT ' + @tableName + '('
	DECLARE @values VARCHAR(MAX) = ') VALUES ('
	SELECT @columns = @columns + COLUMN_NAME + ',', @values=@values + '@' + 
		LOWER(LEFT(COLUMN_NAME, 1)) + RIGHT(COLUMN_NAME, LEN(COLUMN_NAME)-1)  -- 变量名首字母小写
		+ ',' 
	FROM  INFORMATION_SCHEMA.[COLUMNS]  WHERE TABLE_NAME=@tableName AND COLUMN_NAME!='Id'
	SET @columns = LEFT(@columns, LEN(@columns) -1) 
	SET @values = LEFT(@values, LEN(@values) -1)
	
	SET @output += @newline + @columns + @values + ');SELECT SCOPE_IDENTITY()";' + @newline
	
	DECLARE @params VARCHAR(MAX) = '    int objectId = (int)(decimal)DataAccess.GetInstance().ExecuteScalar(sql, CommandType.Text,'
	SELECT @params += @newline + '        new SqlParameter("@' +
	   LOWER(LEFT(COLUMN_NAME, 1)) + RIGHT(COLUMN_NAME, LEN(COLUMN_NAME)-1)  -- 变量名首字母小写
	    + '", entity.'+COLUMN_NAME+'),'
	FROM  INFORMATION_SCHEMA.[COLUMNS]  WHERE TABLE_NAME=@tableName AND COLUMN_NAME!='Id'
	SET @params = LEFT(@params, LEN(@params)-1) + ');'
	
	SET @output += @params + @newline + '    return objectId;'
	SET @output += @newline + '}'

    PRINT @output;
END
