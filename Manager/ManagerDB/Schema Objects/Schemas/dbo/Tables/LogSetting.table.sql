CREATE TABLE [dbo].[LogSetting]
(
	[Id]                  UNIQUEIDENTIFIER   NOT NULL,
	[ParameterName]       NVARCHAR(50)       NOT NULL,
	[TableName]           NVARCHAR(50)       NOT NULL,
	[OldValue]            NVARCHAR(50)       NOT NULL,
	[NewValue]            NVARCHAR(50)       NULL
)
