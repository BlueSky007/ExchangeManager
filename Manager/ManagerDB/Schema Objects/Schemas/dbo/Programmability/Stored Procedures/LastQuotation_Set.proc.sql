CREATE PROCEDURE dbo.LastQuotation_Set
	@instrumentId INT,
	@sourceId INT,
	@timestamp DATETIME2(3),
	@ask FLOAT,
	@bid FLOAT,
	@last FLOAT,
	@high FLOAT,
	@low FLOAT
AS
BEGIN
	IF EXISTS(SELECT lq.InstrumentId FROM LastQuotation lq WHERE lq.InstrumentId=@instrumentId)
	BEGIN
		UPDATE LastQuotation
		SET
		    SourceId = @sourceId,
			[Timestamp] = @timestamp,
			Ask = @ask,
			Bid = @bid,
			[Last] = @last,
			[High] = @high,
			[Low] = @low
		WHERE InstrumentId=@instrumentId
	END
	ELSE
	BEGIN
		INSERT INTO LastQuotation
		(
			InstrumentId,SourceId,[Timestamp],Ask,Bid,[Last],[High],[Low]
		)
		VALUES
		(
			@instrumentId,@sourceId,@timestamp,@ask,@bid,@last,@high,@low
		)		
	END
END