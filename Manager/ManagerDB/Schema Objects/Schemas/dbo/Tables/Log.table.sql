CREATE TABLE [dbo].[Log]
(
	[Id]            UNIQUEIDENTIFIER  NOT NULL,
	[UserId]        UNIQUEIDENTIFIER  NOT NULL,
	[IP]            NVARCHAR (15)     NULL,
	[ExchangeCode]  NVARCHAR(50)      NULL,
    [Event]         NVARCHAR (4000)   NOT NULL,
	[Timestamp]     DATETIME          NOT NULL
)
