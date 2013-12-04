ALTER TABLE [dbo].[DerivativeRelation]
	ADD CONSTRAINT [FK_DerivativeRelation_Instrument] 
	FOREIGN KEY (InstrumentId)
	REFERENCES Instrument (Id)

