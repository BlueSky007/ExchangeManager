CREATE TABLE [dbo].[WeightedPriceRule]
(
	InstrumentId INT NOT NULL, 
	Multiplier DECIMAL(18,9) NOT NULL,
	AskAskWeight INT NOT NULL,
	AskBidWeight INT NOT NULL,
	AskLastWeight INT NOT NULL,
	BidAskWeight INT NOT NULL,
	BidBidWeight INT NOT NULL,
	BidLastWeight INT NOT NULL,
	LastAskWeight INT NOT NULL,
	LastBidWeight INT NOT NULL,
	LastLastWeight INT NOT NULL,
	AskAverageWeight INT NOT NULL,
	BidAverageWeight INT NOT NULL,
	LastAverageWeight INT NOT NULL,
	AskAdjust DECIMAL(18,9) NOT NULL,
	BidAdjust DECIMAL(18,9) NOT NULL,
	LastAdjust DECIMAL(18,9) NOT NULL
)
