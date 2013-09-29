CREATE TABLE [dbo].[LogSourceChange]
(
	[Id]                  UNIQUEIDENTIFIER   NOT NULL,
	[IsDefault]           BIT                NOT NULL,
	[FromSourceId]        UNIQUEIDENTIFIER   NOT NULL,
	[ToSourceId]          UNIQUEIDENTIFIER   NOT NULL,
	[Priority]            INT                NULL
)
