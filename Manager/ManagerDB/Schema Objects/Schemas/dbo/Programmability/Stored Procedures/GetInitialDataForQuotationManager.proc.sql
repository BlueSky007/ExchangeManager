CREATE PROCEDURE [dbo].[GetInitialDataForQuotationManager]
AS
BEGIN
	SET NOCOUNT ON
	SELECT Id, Name, AuthName, [Password] FROM dbo.QuotationSource

	SELECT Id, Code, DecimalPlace, InactiveTime, UseWeightedPrice,
	    IsDerivative, IsSwitchUseAgio, AgioSeconds, LeastTicks FROM dbo.Instrument

	SELECT Id, SourceId, SourceSymbol, InstrumentId, Inverted, IsActive, IsDefault, [Priority], SwitchTimeout,
	    AdjustPoints, AdjustIncrement FROM dbo.InstrumentSourceRelation

	SELECT InstrumentId, UnderlyingInstrument1Id, UnderlyingInstrument1IdInverted, UnderlyingInstrument2Id, 
		AdjustPoints, AdjustIncrement,
		AskOperand1Type, AskOperator1Type,
		AskOperand2Type, AskOperator2Type,
		AskOperand3,
		BidOperand1Type, BidOperator1Type,
		BidOperand2Type, BidOperator2Type,
		BidOperand3,
		LastOperand1Type, LastOperator1Type,
		LastOperand2Type, LastOperator2Type,
		LastOperand3 FROM dbo.DerivativeRelation
	
	SELECT InstrumentId, DiscardOutOfRangePrice, OutOfRangeType, ValidVariation,
	    OutOfRangeWaitTime, OutOfRangeCount FROM dbo.PriceRangeCheckRule
	
	SELECT InstrumentId, Multiplier,
	    AskAskWeight, AskBidWeight, AskLastWeight,
		BidAskWeight, BidBidWeight, BidLastWeight,
		LastAskWeight, LastBidWeight, LastLastWeight,
	    AskAverageWeight, BidAverageWeight, LastAverageWeight,
		AskAdjust, BidAdjust, LastAdjust FROM dbo.WeightedPriceRule
	
	SELECT InstrumentId, SourceId, [Timestamp], Ask, Bid, Last, High, Low FROM dbo.LastQuotation
END
