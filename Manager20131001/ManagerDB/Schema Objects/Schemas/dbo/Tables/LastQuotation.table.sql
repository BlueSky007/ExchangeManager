﻿CREATE TABLE [dbo].[LastQuotation]
(
	SourceId INT NOT NULL, 
	InstrumentId INT NOT NULL,
	[Timestamp] DATETIME2(3) NOT NULL,
	Ask VARCHAR(10) NULL,
	Bid VARCHAR(10) NULL,
	Last VARCHAR(10) NULL,
	High VARCHAR(10) NULL,
	Low VARCHAR(10) NULL
)
