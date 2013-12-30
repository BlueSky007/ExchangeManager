CREATE PROCEDURE [dbo].[P_TaskScheduler_Ins]
(
    @id                    UNIQUEIDENTIFIER,
	@name                  NVARCHAR(50),
	@description           NVARCHAR(50),
	@taskStatus            TINYINT,
	@runTime               DATETIME,
	@lastRunTime           DATETIME,
	@taskType              TINYINT,
	@actionType            TINYINT,
	@interval              INT = NULL,
	@userId                UNIQUEIDENTIFIER,
	@timestamp             DATETIME,
	@parameterSettingsXml  NTEXT=NULL,
	@result                BIT=1 OUTPUT
)
AS
BEGIN
    DECLARE @tranCount INT
	SET @tranCount=@@tranCount
	IF @tranCount=0
		BEGIN TRAN InsertTran
	ELSE
		SAVE TRAN InsertTran

	DECLARE @successful AS BIT
	DECLARE @error INT
	SET @error = 0
	SET @result = 1
	DECLARE @errorMessage AS NVARCHAR(100)
	SET @errorMessage = ''


	DECLARE @parameterSettingTable TABLE
	(
	    [Id]                UNIQUEIDENTIFIER NOT NULL,
	    [ParameterKey]      NVARCHAR(50) NOT NULL,
        [ParameterValue]    NVARCHAR(1000) NOT NULL,
	    [SettingType]       TINYINT        NOT NULL,
		[SqlDbType]         TINYINT        NOT NULL
	)

	IF @parameterSettingsXml IS NOT NULL
	BEGIN
	     DECLARE @doc XML
		 SET @doc = CAST(@parameterSettingsXml AS NVARCHAR(MAX))

		 INSERT INTO @parameterSettingTable
		 (
		    [Id],
		    [ParameterKey],
			[ParameterValue],
			[SettingType],
			[SqlDbType]
		  )
		  SELECT
		  xmlColumn.value('./@Id','UNIQUEIDENTIFIER'),
		  xmlColumn.value('./@ParameterKey','NVARCHAR(50)'),
		  xmlColumn.value('./@ParameterValue','NVARCHAR(1000)'),
		  xmlColumn.value('./@SettingParameterType','TINYINT'),
		  xmlColumn.value('./@SqlDbType','TINYINT') 
		  FROM @doc.nodes('/ParameterSettings/ParameterSetting') AS xmlTable(xmlColumn)

		  INSERT INTO [dbo].[ParameterSettingTask]
           ([Id]
           ,[TaskSchedulerId]
           ,[ParameterKey]
           ,[ParameterValue]
           ,[SettingType]
           ,[SqlDbType])
		 SELECT p.Id,@id,p.ParameterKey,p.ParameterValue,p.SettingType,p.SqlDbType
		 FROM @parameterSettingTable AS p


		SET @error=@@ERROR
		IF @error<>0
		BEGIN
			SET @result = 0
			SET @errorMessage = 'Unknown error, failed to add new ParameterSettingTask'
			GOTO P_Failed
		END
	END


	INSERT INTO [dbo].[TaskScheduler]
           ([Id]
           ,[Name]
		   ,[Description]
           ,[TaskStatus]
		   ,[RunTime]
		   ,[LastRunTime] 
           ,[TaskType]
           ,[ActionType]
           ,[Interval]
		   ,[UserId] 
           ,[Timestamp])
     VALUES
           (@id
           ,@name
		   ,@description
           ,@taskStatus
		   ,@runTime 
		   ,@lastRunTime 
           ,@taskType
           ,@actionType
           ,@interval
		   ,@userId
           ,@timestamp)

	IF @@ROWCOUNT = 0
	BEGIN
        SET @result = 0
	    SET @errorMessage = 'Unknown error, failed to add new TaskScheduler'
		GOTO P_Failed
	END

P_Succeed:
	IF @tranCount=0	COMMIT TRAN InsertTran
	RETURN

P_Failed:
	RAISERROR( @ErrorMessage, 16, 1 )
	ROLLBACK TRAN InsertTran
END