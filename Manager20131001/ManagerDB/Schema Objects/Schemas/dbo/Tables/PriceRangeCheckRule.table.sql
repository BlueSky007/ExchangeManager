CREATE TABLE [dbo].[PriceRangeCheckRule]
(
	InstrumentId INT NOT NULL,
	DiscardOutOfRangePrice BIT NOT NULL,
	OutOfRangeType TINYINT NOT NULL,     -- 0 - Ask, 1 - Bid, 2 - Avarage
	ValidVariation INT NOT NULL,
	OutOfRangeWaitTime INT NOT NULL,     -- sencods
	OutOfRangeCount  INT NOT NULL
)
