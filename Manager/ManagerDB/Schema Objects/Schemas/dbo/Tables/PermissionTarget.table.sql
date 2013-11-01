CREATE TABLE [dbo].[PermissionTarget]
(
	Id           INT IDENTITY(10,1) NOT NULL, 
	ParentId     INT NOT NULL,
	Code	     NVARCHAR(50) NOT NULL,
	TargetType   TINYINT	NOT NULL,
	DataObjectId UNIQUEIDENTIFIER NULL
)
