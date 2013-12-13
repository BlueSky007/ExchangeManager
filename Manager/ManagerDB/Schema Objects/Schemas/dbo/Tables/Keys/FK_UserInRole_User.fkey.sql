ALTER TABLE [dbo].[UserInRole]
	ADD CONSTRAINT [FK_UserInRole_User] 
	FOREIGN KEY (UserId)
	REFERENCES [dbo].[Users] (Id)	

