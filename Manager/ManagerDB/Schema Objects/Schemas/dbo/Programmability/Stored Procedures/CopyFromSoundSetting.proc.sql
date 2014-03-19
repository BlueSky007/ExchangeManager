CREATE PROCEDURE [dbo].[CopyFromSoundSetting]
(
    @userId       UNIQUEIDENTIFIER,
    @copyUserId   UNIQUEIDENTIFIER
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
	DECLARE @errorMessage AS NVARCHAR(100)
	SET @errorMessage = ''

	DECLARE @settingId     UNIQUEIDENTIFIER
	DECLARE @copySettingId UNIQUEIDENTIFIER

	IF EXISTS(SELECT * FROM Settings s
						INNER JOIN SettingsDetail sd ON sd.SettingsId = s.Id
						WHERE sd.SettingType = 4 AND s.UserId = @userId)
	BEGIN
	    DELETE FROM [dbo].[SettingsDetail] WHERE SettingsId = @settingId
	END
	
	IF NOT EXISTS(SELECT * FROM Settings s
						INNER JOIN SettingsDetail sd ON sd.SettingsId = s.Id
						WHERE sd.SettingType = 4 AND s.UserId = @copyUserId)
	BEGIN
	      SET @errorMessage = 'Error:Copy From Setting is empty.'
		  GOTO P_Failed
	END

	IF NOT EXISTS(SELECT * FROM Settings WHERE UserId = @userId)
	BEGIN
	      INSERT INTO Settings(Id,UserId)VALUES(NEWID(),@userId)

		  IF @@ROWCOUNT = 0
		  BEGIN
			 SET @errorMessage = 'Error:Insert settings is failed.'
			 GOTO P_Failed
		  END
	END

	SELECT @copySettingId = Id FROM Settings WHERE UserId = @copyUserId
	SELECT @settingId = Id FROM Settings WHERE UserId = @userId

	INSERT INTO [dbo].[SettingsDetail](Id
		  ,SettingsId
		  ,ParameterKey
		  ,ParameterValue
		  ,SettingType)
	 SELECT NEWID()
		 ,@settingId
		 ,ParameterKey
		 ,ParameterValue
		 ,SettingType
     FROM [dbo].[SettingsDetail] 
	 WHERE SettingsId = @copySettingId
	 AND SettingType = 4

	 IF @@ROWCOUNT = 0
	 BEGIN
	      SET @errorMessage = 'Error:just copy sound settings is failed.'
		  GOTO P_Failed
	 END


	 SELECT sd.Id,sf.SoundType,sd.SettingsId,sd.ParameterKey,sd.ParameterValue,sd.SettingType 
	 FROM SettingsDetail sd
	 LEFT OUTER JOIN SoundDefine sf ON sf.SoundKey = sd.ParameterKey
	 WHERE SettingsId = @settingId
	 AND SettingType = 4

	 SELECT SoundKey,SoundType FROM SoundDefine

P_Succeed:
	IF @tranCount=0	COMMIT TRAN InsertTran
	RETURN

P_Failed:
	RAISERROR( @ErrorMessage, 16, 1 )
	ROLLBACK TRAN InsertTran
END