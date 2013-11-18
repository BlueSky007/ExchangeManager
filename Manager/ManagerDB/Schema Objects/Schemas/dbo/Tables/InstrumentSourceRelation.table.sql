﻿CREATE TABLE [dbo].[InstrumentSourceRelation]
(
	SourceId INT NOT NULL,
	SourceSymbol VARCHAR(20) NOT NULL,
	InstrumentId INT NOT NULL,
	IsActive BIT NOT NULL,
	IsDefault BIT NOT NULL,
	Priority INT NOT NULL,
	SwitchTimeout INT NOT NULL,   -- Senconds
	AdjustPoints DECIMAL(18,9) NOT NULL,
	AdjustIncrement DECIMAL(18,9) NOT NULL,
)
