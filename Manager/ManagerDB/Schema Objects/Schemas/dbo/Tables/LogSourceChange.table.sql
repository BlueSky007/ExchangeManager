CREATE TABLE [dbo].[LogSourceChange]
(
	[Id]                  UNIQUEIDENTIFIER   NOT NULL,
	[FromSourceId]        INT                NOT NULL,
	[ToSourceId]          INT                NOT NULL
)
