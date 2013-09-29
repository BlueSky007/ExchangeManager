CREATE TABLE [dbo].[LogQuote]
(
	[Id]                  UNIQUEIDENTIFIER   NOT NULL,
	[AnswerLot]           DECIMAL(18,4)      NULL,
	[Ask]                 NVARCHAR(10)       NULL,
	[Bid]                 NVARCHAR(10)       NOT NULL,
	[CustomerId]          UNIQUEIDENTIFIER   NOT NULL,
	[InstrumentId]        UNIQUEIDENTIFIER   NOT NULL,
)
