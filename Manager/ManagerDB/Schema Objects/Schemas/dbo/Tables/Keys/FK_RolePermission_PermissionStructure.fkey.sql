ALTER TABLE [dbo].[RolePermission]
	ADD CONSTRAINT [FK_RolePermission_PermissionTarget] 
	FOREIGN KEY (TargetId)
	REFERENCES PermissionTarget (Id)	

