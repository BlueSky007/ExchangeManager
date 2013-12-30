CREATE TABLE [dbo].[SettingsDetail]
(
	Id               UNIQUEIDENTIFIER NOT NULL,  
	SettingsId       UNIQUEIDENTIFIER NOT NULL,
	ParameterKey     NVARCHAR(50),
	ParameterValue   NVARCHAR(1000),
	SettingType      TINYINT,
)
