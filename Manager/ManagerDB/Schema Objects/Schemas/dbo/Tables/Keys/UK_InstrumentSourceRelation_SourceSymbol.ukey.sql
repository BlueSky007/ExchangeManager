ALTER TABLE [dbo].[InstrumentSourceRelation]
    ADD CONSTRAINT [UK_InstrumentSourceRelation_SourceSymbol]
    UNIQUE (InstrumentId,SourceSymbol)