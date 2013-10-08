CREATE PROCEDURE [dbo].[GeneratorEntity]
  @tableName VARCHAR(60),
  @getset BIT = 0
AS
BEGIN
	DECLARE @output AS VARCHAR(MAX);
	DECLARE @newline AS VARCHAR(2) =  CHAR(13) + CHAR(10)
	DECLARE @lines AS TABLE ( line VARCHAR(255) )
	DECLARE @line VARCHAR(255)
	
	SET NOCOUNT ON
	
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
	
	DECLARE @variableName VARCHAR(255) = LOWER(LEFT(@tableName, 1)) + RIGHT(@tableName, LEN(@tableName)-1)
    
    DELETE @lines
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
        
    PRINT @output;
END
