ALTER TABLE [dbo].[UserInRole]
	ADD CONSTRAINT [FK_UserInRole_Roles] 
	FOREIGN KEY (RoleId)
	REFERENCES [dbo].[Roles] (Id)	

