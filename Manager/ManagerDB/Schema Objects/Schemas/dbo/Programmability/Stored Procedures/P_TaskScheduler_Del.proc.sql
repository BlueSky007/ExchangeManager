CREATE PROCEDURE [dbo].[P_TaskScheduler_Del]
(
    @id                     UNIQUEIDENTIFIER,
	@result                 BIT=1   OUTPUT
)
AS
BEGIN
    DECLARE @tranCount INT
	SET @tranCount=@@tranCount
	IF @tranCount=0
		BEGIN TRAN DeleteTran
	ELSE
		SAVE TRAN DeleteTran
	DECLARE @errorMessage AS NVARCHAR(100)
	SET @errorMessage = ''

	SET @result = 1

	DELETE FROM  [dbo].[InstrumentSettings]
	WHERE TaskSchedulerId = @id

	DELETE FROM  [dbo].[ParameterSettingTask]
	WHERE TaskSchedulerId = @id

	DELETE FROM  [dbo].[TaskScheduler]
	WHERE Id = @id

	IF @@ERROR <> 0
	BEGIN
	    SET @result = 0
	    SET @errorMessage = '[TaskScheduler], failed to Delete.'
		GOTO P_Failed
	END

P_Succeed:
	IF @tranCount=0	COMMIT TRAN DeleteTran
    RETURN
P_Failed:
	RAISERROR(@errorMessage, 16, 1 )
	ROLLBACK TRAN DeleteTran
END