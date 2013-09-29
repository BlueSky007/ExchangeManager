CREATE TABLE [dbo].[Instrument]
(
	Id INT NOT NULL IDENTITY(1,1),
	Code VARCHAR(20) NOT NULL,
	MappingCode VARCHAR(20) NOT NULL,
	DecimalPlace INT NOT NULL,
	Inverted BIT NOT NULL,
	InactiveTime INT NOT NULL,
	UseWeightedPrice BIT NOT NULL,
	Multiplier DECIMAL(18,9),
	IsDerivative BIT NOT NULL
)
