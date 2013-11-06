CREATE PROCEDURE dbo.LastQuotation_Set
	@sourceId INT, 
	@instrumentId INT,
	@timestamp DATETIME2(3),
	@ask VARCHAR(10),
	@bid VARCHAR(10),
	@last VARCHAR(10),
	@high VARCHAR(10),
	@low VARCHAR(10)
AS
BEGIN
	IF EXISTS(SELECT lq.SourceId FROM LastQuotation lq WHERE lq.SourceId=@sourceId AND lq.InstrumentId=@instrumentId)
	BEGIN
		UPDATE LastQuotation
		SET
			[Timestamp] = @timestamp,
			Ask = @ask,
			Bid = @bid,
			[Last] = @last,
			[High] = @high,
			[Low] = @low
		WHERE SourceId=@sourceId AND InstrumentId=@instrumentId
	END
	ELSE
	BEGIN
		INSERT INTO LastQuotation
		(
			SourceId,InstrumentId,[Timestamp],Ask,Bid,[Last],[High],[Low]
		)
		VALUES
		(
			@sourceId,@instrumentId,@timestamp,@ask,@bid,@last,@high,@low
		)		
	END
END