﻿CREATE TABLE [dbo].[LastQuotation]
(
	InstrumentId INT NOT NULL,
	SourceId INT NOT NULL,
	[Timestamp] DATETIME2(3) NOT NULL,
	Ask FLOAT NOT NULL,
	Bid FLOAT NOT NULL,
	Last FLOAT NULL,
	High FLOAT NULL,
	Low FLOAT NULL
)
