CREATE PROCEDURE [dbo].[P_LogQuote_Ins]
(
	@id             UNIQUEIDENTIFIER,
	@userId         UNIQUEIDENTIFIER,
	@ip             NVARCHAR(15),
	@exchangeCode   NVARCHAR(50),
	@event          NVARCHAR(4000),
	@timestamp      DATETIME,
	@lot            DECIMAL(18,4),
	@answerLot      DECIMAL(18,4),
	@ask            NVARCHAR(10),
	@bid            NVARCHAR(10),
	@isBuy          BIT,
	@customerId     UNIQUEIDENTIFIER,
	@customerName   NVARCHAR(50)= NULL,
	@instrumentId   UNIQUEIDENTIFIER,
	@instrumentCode NVARCHAR(50) = NULL,
	@sendTime       DATETIME
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
	    SET @errorMessage = 'Unknown error, failed to add new [LogQuote].[Log]'
		GOTO P_Failed
	END

   INSERT INTO [dbo].[LogQuote]([Id],[Lot],[AnswerLot],[Ask],[Bid] ,[IsBuy],[CustomerId],[CustomerName],[InstrumentId],[InstrumentCode],[SendTime])
   VALUES(@id,@lot,@answerLot,@ask,@bid,@isBuy,@customerId,@customerName,@instrumentId,@instrumentCode,@sendTime)

    IF @@ROWCOUNT = 0
	BEGIN
	    SET @errorMessage = 'Unknown error, failed to add new [LogQuote]'
		GOTO P_Failed
	END


P_Succeed:
	IF @tranCount=0	COMMIT TRAN InsertTran
	RETURN

P_Failed:
	RAISERROR( @ErrorMessage, 16, 1 )
	ROLLBACK TRAN InsertTran
END