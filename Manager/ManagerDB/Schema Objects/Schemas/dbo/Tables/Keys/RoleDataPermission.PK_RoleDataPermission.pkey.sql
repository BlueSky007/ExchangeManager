ALTER TABLE [dbo].[RoleDataPermission]
	ADD CONSTRAINT [PK_RoleDataPermission]
	PRIMARY KEY (RoleId,IExchangeCode,DataObjectType,DataObjectId)