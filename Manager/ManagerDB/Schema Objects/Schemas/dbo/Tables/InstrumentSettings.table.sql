CREATE TABLE [dbo].[InstrumentSettings]
(
	Id                 UNIQUEIDENTIFIER  NOT NULL, 
	TaskSchedulerId    UNIQUEIDENTIFIER  NOT NULL, 
	ExchangeCode       NVARCHAR(50)      NOT NULL,
	InstrumentId       UNIQUEIDENTIFIER  NOT NULL, 
	InstrumentCode     NVARCHAR(50)      NOT NULL,
)
