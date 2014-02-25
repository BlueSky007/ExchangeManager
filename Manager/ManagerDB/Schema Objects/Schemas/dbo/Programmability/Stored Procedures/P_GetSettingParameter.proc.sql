CREATE PROCEDURE [dbo].[P_GetSettingParameter]
(
	@userId  UNIQUEIDENTIFIER
)
AS
BEGIN
    SELECT sd.Id,sd.SettingsId,sd.ParameterKey,sd.ParameterValue,sd.SettingType,sf.SoundType 
	FROM SettingsDetail sd
	INNER JOIN Settings s ON s.Id = sd.SettingsId
	LEFT OUTER JOIN SoundDefine sf ON sf.SoundKey = sd.ParameterKey
	WHERE s.UserId = @userId

	SELECT SoundKey,SoundType FROM SoundDefine
END