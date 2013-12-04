CREATE TABLE [dbo].[PermissionTarget]
(
	Id           INT IDENTITY(10,1) NOT NULL, --0 Root, 1 FunctionPermission  ,2 DataPermission
	ParentId     INT NOT NULL,   
	Code	     NVARCHAR(50) NOT NULL,
	[Level]		 TINYINT	NOT NULL, -- 1 界面显示时第一层，2 界面显示时第二层 ......
	TargetType   TINYINT	NOT NULL, -- 1 FunctionPermission,2 DataPermission
	DataObjectId UNIQUEIDENTIFIER NULL -- WHEN TargetType=2 NOT NULL
)
