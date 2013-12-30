CREATE TABLE [dbo].[SettingLog]
(
	[ID]                  UNIQUEIDENTIFIER   NOT NULL,
	[Code]                NVARCHAR(20)       NOT NULL,
	[ParameterName]       NVARCHAR(20)       NOT NULL,
	[TableName]           NVARCHAR(20)       NOT NULL,
	[NewValue]            NVARCHAR(50)       NULL
)
