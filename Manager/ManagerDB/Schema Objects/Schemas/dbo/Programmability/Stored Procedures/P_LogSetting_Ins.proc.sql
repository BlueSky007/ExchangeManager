CREATE PROCEDURE [dbo].[P_LogSetting_Ins]
(
	@id             UNIQUEIDENTIFIER,
	@userId         UNIQUEIDENTIFIER,
	@ip             NVARCHAR(15),
	@exchangeCode   NVARCHAR(50),
	@event          NVARCHAR(4000),
	@timestamp      DATETIME,
	@parameterName  NVARCHAR(50),
	@tableName      NVARCHAR(50),
	@oldValue       NVARCHAR(50),
	@newValue       NVARCHAR(50)
)
AS
BEGIN
    DECLARE @tranCount INT
	SET @tranCount=@@tranCount
	IF @tranCount=0
		BEGIN TRAN InsertTran
	ELSE
		SAVE TRAN InsertTran

	DECLARE @successful AS BIT
	SET @successful = 1
	DECLARE @errorMessage AS NVARCHAR(100)
	SET @errorMessage = ''


	INSERT INTO [dbo].[Log]([Id],[UserId],[IP],[ExchangeCode],[Event],[Timestamp])
    VALUES(@id,@userId,@ip,@exchangeCode,@event,@timestamp)

	IF @@ROWCOUNT = 0
	BEGIN
	    SET @errorMessage = 'Unknown error, failed to add new [LogSetting].[Log]'
		GOTO P_Failed
	END

   INSERT INTO [dbo].[LogSetting]([Id]
           ,[ParameterName]
           ,[TableName]
           ,[OldValue]
           ,[NewValue])
    VALUES
           (@id
           ,@parameterName
           ,@tableName
           ,@oldValue
           ,@newValue)

    IF @@ROWCOUNT = 0
	BEGIN
	    SET @errorMessage = 'Unknown error, failed to add new [LogSetting]'
		GOTO P_Failed
	END


P_Succeed:
	IF @tranCount=0	COMMIT TRAN InsertTran
	RETURN

P_Failed:
	RAISERROR( @ErrorMessage, 16, 1 )
	ROLLBACK TRAN InsertTran
END