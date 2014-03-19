CREATE TYPE [dbo].[PermissionTargetType] AS TABLE 
(
	Id	INT NULL,
	ParentId	INT NULL,
	[Level]		INT NOT NULL,
	Code		NVARCHAR(50) NULL,
	ExchangeCode	NVARCHAR(50) NULL,
	TargetType	INT NOT NULL,
	DataObjectType	NVARCHAR(20) NULL,
	DataObjectId	UNIQUEIDENTIFIER NULL,
	[Status]	BIT NOT NULL
)
