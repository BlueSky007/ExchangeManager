CREATE PROCEDURE [dbo].[P_LogOrder_Ins]
(
    @id                 UNIQUEIDENTIFIER,
	@userId             UNIQUEIDENTIFIER,
	@ip                 NVARCHAR(15),
	@exchangeCode       NVARCHAR(50),
	@event              NVARCHAR(4000),
	@timestamp          DATETIME,
	@operationType      TINYINT,
	@orderId            UNIQUEIDENTIFIER,
	@orderCode          NVARCHAR(50),
	@accountCode        NVARCHAR(20),
	@instrumentCode     NVARCHAR(50),
	@isBuy              BIT,
	@isOpen             BIT,
	@lot                DECIMAL(18,4),
	@setPrice           NVARCHAR(20),
	@orderTypeId        INT,
	@orderRelation      NVARCHAR(4000)=NULL,
	@transactionCode    NVARCHAR(50)
)
AS
BEGIN
    DECLARE @tranCount INT
	SET @tranCount=@@tranCount
	IF @tranCount=0
		BEGIN TRAN InsertTran
	ELSE
		SAVE TRAN InsertTran

	DECLARE @successful AS BIT
	SET @successful = 1
	DECLARE @errorMessage AS NVARCHAR(100)
	SET @errorMessage = ''


	INSERT INTO [dbo].[Log]([Id],[UserId],[IP],[ExchangeCode],[Event],[Timestamp])
    VALUES(@id,@userId,@ip,@exchangeCode,@event,@timestamp)

	IF @@ROWCOUNT = 0
	BEGIN
	    SET @errorMessage = 'Unknown error, failed to add new [LogOrder].[Log]'
		GOTO P_Failed
	END

   INSERT INTO [dbo].[LogOrder]([Id],[OperationType],[OrderId],[OrderCode],[AccountCode],[InstrumentCode],[IsBuy],[IsOpen],[Lot],[SetPrice],[OrderTypeId],[OrderRelation],[TransactionCode])
   VALUES(@id,@operationType,@orderId,@orderCode,@accountCode,@instrumentCode,@isBuy,@isOpen,@lot,@setPrice,@orderTypeId,@orderRelation,@transactionCode)

    IF @@ROWCOUNT = 0
	BEGIN
	    SET @errorMessage = 'Unknown error, failed to add new [LogOrder]'
		GOTO P_Failed
	END


P_Succeed:
	IF @tranCount=0	COMMIT TRAN InsertTran
	RETURN

P_Failed:
	RAISERROR( @ErrorMessage, 16, 1 )
	ROLLBACK TRAN InsertTran
END