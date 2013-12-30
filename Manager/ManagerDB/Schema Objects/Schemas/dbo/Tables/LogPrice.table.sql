CREATE TABLE [dbo].[LogPrice]
(
	 [Id]                  UNIQUEIDENTIFIER   NOT NULL,
	 [InstrumentId]        INT                NOT NULL,
	 [InstrumentCode]      NVARCHAR(20)       NOT NULL,
	 [OperationType]       TINYINT            NOT NULL,
	 [Price]               NVARCHAR(20)       NULL,
	 [Diff]                NVARCHAR(20)       NULL,
)
