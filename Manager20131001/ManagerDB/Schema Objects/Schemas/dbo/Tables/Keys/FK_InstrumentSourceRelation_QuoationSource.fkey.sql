ALTER TABLE [dbo].[InstrumentSourceRelation]
	ADD CONSTRAINT [FK_InstrumentSource_QuoationSource] 
	FOREIGN KEY (SourceId)
	REFERENCES QuotationSource (Id)	

