ALTER TABLE [dbo].[InstrumentSourceRelation]
	ADD CONSTRAINT [PK_InstrumentSource]
	PRIMARY KEY (SourceId,SourceSymbol,InstrumentId)