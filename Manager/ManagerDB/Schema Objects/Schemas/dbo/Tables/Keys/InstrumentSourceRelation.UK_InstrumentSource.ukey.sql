ALTER TABLE [dbo].[InstrumentSourceRelation]
    ADD CONSTRAINT [UK_InstrumentSource]
    UNIQUE (SourceId,SourceSymbol,InstrumentId)