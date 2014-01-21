CREATE PROCEDURE [dbo].[P_SettingsDetailParameter_Upd]
(
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

	DECLARE @settingDetailId UNIQUEIDENTIFIER
	SET @settingDetailId = NEWID()

	DECLARE @settingDetailsTable TABLE
	( 
	    [SettingId]         UNIQUEIDENTIFIER  NOT NULL,
	    [ParameterKey]      NVARCHAR(50)    NOT NULL,
        [ParameterValue]    NVARCHAR(1000)  NOT NULL,
	    [SettingType]       TINYINT         NOT NULL
	)

	IF @settingDetailsXml IS NOT NULL
	BEGIN
	    DECLARE @doc XML
		SET @doc = CAST(@settingDetailsXml AS NVARCHAR(MAX))
		INSERT INTO @settingDetailsTable
		(
		   [SettingId] ,
		   [ParameterKey],
		   [ParameterValue],
		   [SettingType] 
		)
		SELECT 
		  xmlColumn.value('./@SettingId','UNIQUEIDENTIFIER'),
		  xmlColumn.value('./@ParameterKey','NVARCHAR(50)'),
		  xmlColumn.value('./@ParameterValue','NVARCHAR(1000)'),
		  xmlColumn.value('./@SettingParameterType','TINYINT')
		FROM @doc.nodes('/SettingDetails/SettingDetail') AS xmlTable(xmlColumn)

		UPDATE [dbo].[SettingsDetail]
        SET [ParameterKey] = p.ParameterKey,[ParameterValue] = p.ParameterValue
	    FROM [dbo].[SettingsDetail] sd
		INNER JOIN @settingDetailsTable p ON P.ParameterKey = sd.ParameterKey

		SET @error=@@ERROR
		IF @error<>0
		BEGIN
			SET @result = 0
			SET @errorMessage = 'Unknown error, failed to update new SettingsDetail'
			GOTO P_Failed
		END

		IF EXISTS(SELECT * FROM @settingDetailsTable sdt WHERE sdt.ParameterKey NOT IN(SELECT ParameterKey FROM dbo.SettingsDetail))
		BEGIN
		    INSERT INTO [dbo].[SettingsDetail](Id,SettingsId,ParameterKey,ParameterValue,SettingType)
			SELECT NEWID(),SettingId,ParameterKey,ParameterValue,SettingType 
			FROM @settingDetailsTable sd
			WHERE sd.ParameterKey NOT IN(SELECT ParameterKey FROM dbo.SettingsDetail)
			
			SET @error=@@ERROR
			IF @error<>0
			BEGIN
				SET @result = 0
				SET @errorMessage = 'Unknown error, failed to add new SettingsDetail'
				GOTO P_Failed
			END
		END
	END

P_Succeed:
   IF @tranCount = 0 COMMIT TRAN UpdateTran
   RETURN
P_Failed:
	RAISERROR( @errorMessage, 16, 1 )
	ROLLBACK TRAN UpdateTran
END