ALTER TABLE [dbo].[FunctionDescription]
	ADD CONSTRAINT [FK_FunctionDescription_PermissionTarget] 
	FOREIGN KEY (FunctionId)
	REFERENCES PermissionTarget (Id)	

