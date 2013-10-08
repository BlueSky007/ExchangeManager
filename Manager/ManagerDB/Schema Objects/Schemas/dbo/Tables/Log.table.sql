CREATE TABLE [dbo].[Log]
(
	[Id]            UNIQUEIDENTIFIER NOT NULL,
	[UserId]        UNIQUEIDENTIFIER NOT NULL,
	[IP]            NVARCHAR (15)    NULL,
    [Event]         NVARCHAR (4000)  NOT NULL,
	[Timestamp]     DATETIME         NOT NULL
)
