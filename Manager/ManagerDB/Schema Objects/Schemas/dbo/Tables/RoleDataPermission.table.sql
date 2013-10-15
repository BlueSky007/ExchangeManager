CREATE TABLE [dbo].[RoleDataPermission]
(
	RoleId INT NOT NULL, 
	IExchangeCode NVARCHAR(50) NOT NULL,
	[DataObjectType]	TINYINT NOT NULL,
	DataObjectId UNIQUEIDENTIFIER NOT NULL,
	DataObjectDescription	NVARCHAR(255) NOT NULL
)
