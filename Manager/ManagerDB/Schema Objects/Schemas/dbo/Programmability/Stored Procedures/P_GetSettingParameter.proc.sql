CREATE PROCEDURE [dbo].[P_GetSettingParameter]
(
	@userId  UNIQUEIDENTIFIER
)
AS
BEGIN
    SELECT sd.Id,sd.SettingsId,sd.ParameterKey,sd.ParameterValue,sd.SettingType 
	FROM SettingsDetail sd
	INNER JOIN Settings s ON s.Id = sd.SettingsId
	WHERE s.UserId = @userId
END