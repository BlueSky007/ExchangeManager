CREATE TABLE [dbo].[ParameterSettingTask]
( 
    Id                 UNIQUEIDENTIFIER NOT NULL, 
	TaskSchedulerId    UNIQUEIDENTIFIER NOT NULL, 
	ParameterKey       NVARCHAR(50)   NOT NULL,
	ParameterValue     NVARCHAR(1000) NOT NULL,
	SettingType        TINYINT        NOT NULL,
    SqlDbType          TINYINT        NOT NULL
)
