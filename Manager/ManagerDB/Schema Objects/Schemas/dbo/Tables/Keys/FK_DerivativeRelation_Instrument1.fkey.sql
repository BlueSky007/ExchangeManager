ALTER TABLE [dbo].[DerivativeRelation]
	ADD CONSTRAINT [FK_DerivativeRelation_Instrument1] 
	FOREIGN KEY (UnderlyingInstrument1Id)
	REFERENCES Instrument (Id)

