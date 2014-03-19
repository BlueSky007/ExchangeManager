ALTER TABLE [dbo].[AdjustRelation]
	ADD CONSTRAINT [FK_AdjustRelation_Instrument] 
	FOREIGN KEY (InstrumentId)
	REFERENCES Instrument (Id)	

