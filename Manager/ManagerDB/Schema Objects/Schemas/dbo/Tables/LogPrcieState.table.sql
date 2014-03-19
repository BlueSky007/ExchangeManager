CREATE TABLE [dbo].[LogPrcieState]
(
	ExchangeCode INT NOT NULL, 
	InstrumentId UNIQUEIDENTIFIER NOT NULL,
	IsPriceEnabled BIT NOT NULL,
	IsAutoEnablePrice BIT NOT NULL
)
