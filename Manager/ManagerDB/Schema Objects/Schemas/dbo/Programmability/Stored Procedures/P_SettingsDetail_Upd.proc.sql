CREATE PROCEDURE [dbo].[P_SettingsDetail_Upd]
(
    @taskSchedulerId       UNIQUEIDENTIFIER,
	@runTime               DATETIME,
	@lastRunTime           DATETIME,
	@settingDetailsXml     NTEXT=NULL,
	@result                BIT=1 OUTPUT
)
AS
BEGIN
    DECLARE @tranCount INT
	SET @tranCount=@@tranCount
	IF @tranCount=0
		BEGIN TRAN UpdateTran
	ELSE
		SAVE TRAN UpdateTran

	DECLARE @successful AS BIT
	DECLARE @error INT
	SET @error = 0
	SET @result = 1
	DECLARE @errorMessage AS NVARCHAR(100)
	SET @errorMessage = ''

	DECLARE @settingDetailsTable TABLE
	(
	    [UserId]            UNIQUEIDENTIFIER NOT NULL,
	    [ParameterKey]      NVARCHAR(50) NOT NULL,
        [ParameterValue]    NVARCHAR(1000) NOT NULL,
	    [SettingType]       TINYINT        NOT NULL
	)

	IF @settingDetailsXml IS NOT NULL
	BEGIN
	    DECLARE @doc XML
		SET @doc = CAST(@settingDetailsXml AS NVARCHAR(MAX))
		INSERT INTO @settingDetailsTable
		(
		   [UserId],
		   [ParameterKey],
		   [ParameterValue],
		   [SettingType] 
		)
		SELECT 
		  xmlColumn.value('./@UserId','UNIQUEIDENTIFIER'),
		  xmlColumn.value('./@ParameterKey','NVARCHAR(50)'),
		  xmlColumn.value('./@ParameterValue','NVARCHAR(1000)'),
		  xmlColumn.value('./@SettingParameterType','TINYINT')
		FROM @doc.nodes('/SettingDetails/SettingDetail') AS xmlTable(xmlColumn)


		UPDATE [dbo].[SettingsDetail]
        SET [ParameterKey] = p.ParameterKey,[ParameterValue] = p.ParameterValue
	    FROM [dbo].[SettingsDetail] sd
		INNER JOIN dbo.Settings s ON s.Id = sd.SettingsId
		INNER JOIN @settingDetailsTable p ON p.ParameterKey = sd.ParameterKey
				AND s.UserId = p.UserId

		SET @error=@@ERROR
		IF @error<>0
		BEGIN
			SET @result = 0
			SET @errorMessage = 'Unknown error, failed to add new SettingsDetail'
			GOTO P_Failed
		END

		UPDATE [dbo].[TaskScheduler] 
        SET TaskStatus = 1,RunTime = @runTime,LastRunTime = @lastRunTime
		WHERE Id = @taskSchedulerId

		SET @error=@@ERROR
		IF @error<>0
		BEGIN
			SET @result = 0
			SET @errorMessage = 'Unknown error, failed to update [TaskScheduler] TaskStatus'
			GOTO P_Failed
		END
	END

P_Succeed:
   IF @tranCount = 0 COMMIT TRAN UpdateTran
   RETURN
P_Failed:
	RAISERROR( @errorMessage, 16, 1 )
	ROLLBACK TRAN UpdateTran
END