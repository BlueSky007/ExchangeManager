﻿CREATE PROCEDURE [dbo].[UpdateAdjustRelation]
(
	@Id	UNIQUEIDENTIFIER,
	@Code	NVARCHAR(30),
	@InstrumentIds	NVARCHAR(MAX)
)
AS
BEGIN
	DECLARE @tranCount INT,
			@error INT =0
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
	SET @tranCount=@@tranCount
	IF @tranCount=0
		BEGIN TRAN Tran1
	ELSE
		SAVE TRAN Tran1
	
	DECLARE @Instrument	TABLE
	(
		instrumentId	INT
	)
	
	DECLARE @CurrentIndex int;
    DECLARE @NextIndex int;
    DECLARE @ReturnText nvarchar(max);
	SELECT @CurrentIndex=1;
    WHILE(@CurrentIndex<=len(@InstrumentIds))    
	BEGIN
		SELECT @NextIndex=charindex(',',@InstrumentIds,@CurrentIndex);
		IF(@NextIndex=0 OR @NextIndex IS NULL)
		SELECT @NextIndex=len(@InstrumentIds)+1;
		SELECT @ReturnText=substring(@InstrumentIds,@CurrentIndex,@NextIndex-@CurrentIndex);
		INSERT INTO @Instrument(instrumentId) VALUES(@ReturnText);
		SELECT @CurrentIndex=@NextIndex+1;
		SET @error=@@ERROR
		IF @error<>0 GOTO P_Falied
	END
	
	INSERT INTO AdjustRelation(Id,Code,InstrumentId)
	SELECT @Id,@Code,i.instrumentId
	FROM @Instrument i
	SET @error=@@ERROR
	IF @error<>0 GOTO P_Falied
	
P_Succeed:
	IF @tranCount=0	COMMIT TRAN Tran1
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED
	SET @error=0
	GOTO P_End
	  
P_Falied:
	ROLLBACK TRAN Tran1
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED
	GOTO P_End	

P_End:			
	RETURN @error
END