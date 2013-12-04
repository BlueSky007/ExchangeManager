ALTER TABLE [dbo].[PriceRangeCheckRule]
	ADD CONSTRAINT [FK_PriceRangeCheckRule_Instrument] 
	FOREIGN KEY (InstrumentId)
	REFERENCES Instrument (Id)	

