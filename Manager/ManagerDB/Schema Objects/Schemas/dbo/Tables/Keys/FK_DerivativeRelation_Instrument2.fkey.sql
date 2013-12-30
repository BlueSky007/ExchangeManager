ALTER TABLE [dbo].[DerivativeRelation]
	ADD CONSTRAINT [FK_DerivativeRelation_Instrument2] 
	FOREIGN KEY (UnderlyingInstrument2Id)
	REFERENCES Instrument (Id)

