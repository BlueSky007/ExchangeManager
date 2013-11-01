ALTER TABLE [dbo].[RolePermission]
	ADD CONSTRAINT [PK_RolePermission]
	PRIMARY KEY (RoleId,TargetId)