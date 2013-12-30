CREATE TABLE [dbo].[DerivativeRelation]
(
	InstrumentId INT NOT NULL, 
	UnderlyingInstrument1Id INT NOT NULL,
	UnderlyingInstrument1IdInverted BIT NOT NULL,  -- 决定Operand1是否取倒数
	UnderlyingInstrument2Id INT NULL,

	AskOperand1Type TINYINT NOT NULL,             -- 0 - Ask, 1 - Bid, 2 - Last
	AskOperator1Type TINYINT NULL,                -- 0 - x, 1 - ÷
	AskOperand2Type TINYINT NULL,
	AskOperator2Type TINYINT NOT NULL,
	AskOperand3 DECIMAL(18,9) NOT NULL,

	BidOperand1Type TINYINT NOT NULL,             -- 0 - Ask, 1 - Bid, 2 - Last
	BidOperator1Type TINYINT NULL,                -- 0 - x, 1 - ÷
	BidOperand2Type TINYINT NULL,
	BidOperator2Type TINYINT NOT NULL,
	BidOperand3 DECIMAL(18,9) NOT NULL,

	LastOperand1Type TINYINT NOT NULL,             -- 0 - Ask, 1 - Bid, 2 - Last
	LastOperator1Type TINYINT NULL,                -- 0 - x, 1 - ÷
	LastOperand2Type TINYINT NULL,
	LastOperator2Type TINYINT NOT NULL,
	LastOperand3 DECIMAL(18,9) NOT NULL,
)
