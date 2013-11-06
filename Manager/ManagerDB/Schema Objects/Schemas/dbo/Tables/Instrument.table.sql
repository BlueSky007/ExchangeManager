CREATE TABLE [dbo].[Instrument]
(
	Id INT NOT NULL IDENTITY(1,1),
	Code VARCHAR(20) NOT NULL,
	MappingCode VARCHAR(20) NOT NULL,
	DecimalPlace INT NOT NULL,
	Inverted BIT NOT NULL,
	InactiveTime INT NOT NULL,
	UseWeightedPrice BIT NOT NULL,
	IsDerivative BIT NOT NULL,
	IsSwitchUseAgio BIT NOT NULL,
	AgioSeconds INT NULL,
	LeastTicks INT NULL  -- 计算价差时在 AgioSeconds 秒内至少要收到的价格数。
)
