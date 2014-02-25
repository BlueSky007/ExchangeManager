CREATE TABLE [dbo].[LogQuote]
(
	[Id]                  UNIQUEIDENTIFIER   NOT NULL,
	[Lot]                 DECIMAL(18,4)      NULL,
	[AnswerLot]           DECIMAL(18,4)      NULL,
	[Ask]                 NVARCHAR(10)       NULL,
	[Bid]                 NVARCHAR(10)       NOT NULL,
	[IsBuy]               BIT                NOT NULL,
	[CustomerId]          UNIQUEIDENTIFIER   NOT NULL,
	[CustomerName]        NVARCHAR(50)       NULL,
	[InstrumentId]        UNIQUEIDENTIFIER   NOT NULL,
	[InstrumentCode]      NVARCHAR(50)       NOT NULL,
	[SendTime]            DATETIME           NOT NULL
)
