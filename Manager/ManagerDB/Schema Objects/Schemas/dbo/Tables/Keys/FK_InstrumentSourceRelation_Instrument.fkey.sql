ALTER TABLE [dbo].[InstrumentSourceRelation]
	ADD CONSTRAINT [FK_InstrumentSource_Instrument] 
	FOREIGN KEY (InstrumentId)
	REFERENCES Instrument (Id)	

