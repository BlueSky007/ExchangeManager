CREATE TABLE [dbo].[LogSourceChange]
(
	[Id]                  UNIQUEIDENTIFIER   NOT NULL,
	[IsDefault]           BIT                NOT NULL,
	[FromSourceId]        INT                NOT NULL,
	[ToSourceId]          INT                NOT NULL,
	[Priority]            TINYINT                NULL
)
