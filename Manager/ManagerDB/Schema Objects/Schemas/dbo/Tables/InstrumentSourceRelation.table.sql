CREATE TABLE [dbo].[InstrumentSourceRelation]
(
    Id INT NOT NULL IDENTITY(1,1),
	SourceId INT NOT NULL,
	SourceSymbol VARCHAR(20) NOT NULL,
	InstrumentId INT NOT NULL,
	Inverted BIT NOT NULL,
	IsActive BIT NOT NULL,
	IsDefault BIT NOT NULL,
	Priority INT NOT NULL,
	SwitchTimeout INT NOT NULL,   -- Senconds
	AdjustPoints FLOAT NOT NULL,
	AdjustIncrement FLOAT NOT NULL,
)
