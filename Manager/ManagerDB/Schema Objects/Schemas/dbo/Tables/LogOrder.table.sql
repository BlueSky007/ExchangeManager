CREATE TABLE [dbo].[LogOrder]
(
    [Id]                  UNIQUEIDENTIFIER   NOT NULL,
	[OperationType]       TINYINT            NOT NULL,
	[AccountId]           UNIQUEIDENTIFIER   NOT NULL,
	[InstrumentId]        UNIQUEIDENTIFIER   NOT NULL,
	[IsBuy]               BIT                NOT NULL,
	[IsOpen]              BIT                NOT NULL,
	[Lot]                 DECIMAL(18, 4)     NOT NULL,
	[SetPrice]            NVARCHAR(10)       NULL,
	[OrderTypeId]         INT                NOT NULL,
	[OrderRelation]       NVARCHAR(4000)     NULL,
	[TransactionId]       UNIQUEIDENTIFIER   NULL,
)
