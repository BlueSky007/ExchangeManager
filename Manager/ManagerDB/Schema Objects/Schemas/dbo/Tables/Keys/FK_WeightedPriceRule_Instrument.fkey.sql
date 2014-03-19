ALTER TABLE [dbo].[WeightedPriceRule]
	ADD CONSTRAINT [FK_WeightedPriceRule_Instrument] 
	FOREIGN KEY (InstrumentId)
	REFERENCES Instrument (Id)	

