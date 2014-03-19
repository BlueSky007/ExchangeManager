CREATE TABLE [dbo].[Instrument]
(
	Id INT NOT NULL IDENTITY(1,1),
	Code VARCHAR(20) NOT NULL,
	AdjustPoints INT NOT NULL,
	AdjustIncrement INT NOT NULL,
	DecimalPlace INT NOT NULL,
	IsDerivative BIT NOT NULL,

	-- 以下属性只对非衍生Instrument有效
	UseWeightedPrice BIT NULL,
	InactiveTime INT NULL,
	IsSwitchUseAgio BIT NULL,  -- 价格源切换时是否考虑价差
	AgioSeconds INT NULL,
	LeastTicks INT NULL        -- 计算价差时在 AgioSeconds 秒内至少要收到的价格数。
)
