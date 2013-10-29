CREATE TYPE [dbo].[DataPermissionsTableType] AS TABLE 
(
	ExchangeCode	NVARCHAR(50),
	DataObjectType	TINYINT,
	DataObjectId	UNIQUEIDENTIFIER,
	DataObjectDescription	NVARCHAR(255)
)