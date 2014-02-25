CREATE PROCEDURE [dbo].[Instrument_Delete]
	@instrumentId int
AS
BEGIN
	BEGIN TRAN
	BEGIN TRY
		DELETE FROM WeightedPriceRule WHERE InstrumentId=@instrumentId
		DELETE FROM PriceRangeCheckRule WHERE InstrumentId=@instrumentId
		DELETE FROM DerivativeRelation WHERE InstrumentId=@instrumentId
		DELETE FROM InstrumentSourceRelation WHERE InstrumentId=@instrumentId
		DELETE FROM Instrument WHERE Id=@instrumentId
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		/* 
			SELECT ERROR_NUMBER() AS ErrorNumber, ERROR_SEVERITY() AS ErrorSeverity,
				ERROR_STATE() AS ErrorState, ERROR_PROCEDURE() AS ErrorProcedure,
				ERROR_LINE() AS ErrorLine, ERROR_MESSAGE() AS ErrorMessage
		*/
		ROLLBACK TRAN
		DECLARE @ErrorMessage NVARCHAR(4000);
        DECLARE @ErrorSeverity INT;
        DECLARE @ErrorState INT;

        SELECT @ErrorMessage = 'Line:' + CAST(ERROR_LINE() AS VARCHAR) + ', ErrorMessage: ' + ERROR_MESSAGE();
        SELECT @ErrorSeverity = ERROR_SEVERITY();
        SELECT @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END


