CREATE PROCEDURE [dbo].[GetInitialDataForTaskScheduler]
AS
BEGIN
    DECLARE @currentTime DATETIME
	SET @currentTime = GETDATE()
    SELECT [Id],[Name],[Description],[TaskStatus],[RunTime],[LastRunTime],[TaskType],[ActionType],[Interval],[UserId],[Timestamp] 
	FROM [dbo].[TaskScheduler] WHERE [RunTime] > @currentTime

	SELECT [Id],[TaskSchedulerId],[ParameterKey] ,[ParameterValue],[SettingType],[SqlDbType] 
	FROM [dbo].[ParameterSettingTask]

	SELECT [Id],[TaskSchedulerId],[ExchangeCode],[InstrumentId],[InstrumentCode]
    FROM   [dbo].[InstrumentSettings]
END