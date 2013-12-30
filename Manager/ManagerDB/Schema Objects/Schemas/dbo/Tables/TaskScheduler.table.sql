CREATE TABLE [dbo].[TaskScheduler]
(
	Id              UNIQUEIDENTIFIER  NOT NULL,  
	Name            NVARCHAR(50)      NOT NULL,
    [Description]   NVARCHAR(255)     NOT NULL,
	TaskStatus      TINYINT           NOT NULL,
	RunTime         DATETIME          NOT NULL,
	LastRunTime     DATETIME          NULL,
	TaskType        TINYINT           NOT NULL,
	ActionType      TINYINT           NOT NULL,
	Interval        INT                  NULL,
	[UserId]        UNIQUEIDENTIFIER  NOT NULL,
	[Timestamp]     DATETIME          NOT NULL,
)
