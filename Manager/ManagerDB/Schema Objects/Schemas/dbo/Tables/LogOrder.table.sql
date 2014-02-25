CREATE TABLE [dbo].[LogOrder]
(
    [Id]                  UNIQUEIDENTIFIER   NOT NULL,
	[OperationType]       TINYINT            NOT NULL,
	[OrderId]             UNIQUEIDENTIFIER   NOT NULL,
	[OrderCode]           NVARCHAR(50)       NOT NULL,
	[AccountCode]         NVARCHAR(20)       NOT NULL,
	[InstrumentCode]      NVARCHAR(20)       NOT NULL,
	[IsBuy]               BIT                NOT NULL,
	[IsOpen]              BIT                NOT NULL,
	[Lot]                 DECIMAL(18, 4)     NOT NULL,
	[SetPrice]            NVARCHAR(10)       NULL,
	[OrderTypeId]         INT                NOT NULL,
	[OrderRelation]       NVARCHAR(4000)     NULL,
	[TransactionCode]     NVARCHAR(50)       NULL,
)
