	CREATE PROCEDURE dbo.UpdateRolePermission
(
	@roleId INT,
	@roleName	NVARCHAR(255) = NULL,
	@xmlAccessTree	NTEXT = NULL,
	@xmlDataTree	NTEXT = NULL
)
AS 
BEGIN
	DECLARE @error INT = 0
	
	DECLARE @accessPermissions TABLE 
	(
		Id	UNIQUEIDENTIFIER
	) 
	DECLARE @dataPermissions TABLE 
	(
		Id	UNIQUEIDENTIFIER
	)
	
	DECLARE @idoc INT
	
	EXEC sp_xml_preparedocument @idoc OUTPUT, @xmlAccessTree
	INSERT INTO @accessPermissions(Id)
	SELECT [Id] FROM OPENXML(@idoc,'/AccessTree/Access',1)
	WITH ( [Id] UNIQUEIDENTIFIER ) x
	EXEC sp_xml_removedocument @idoc
	
	EXEC sp_xml_preparedocument @idoc OUTPUT, @xmlDataTree
	INSERT INTO @dataPermissions(Id)
	SELECT [Id] FROM OPENXML(@idoc,'/DataTree/Data',1)
	WITH ( [Id] UNIQUEIDENTIFIER ) x
	EXEC sp_xml_removedocument @idoc
	
	IF(@xmlAccessTree IS NULL)
	BEGIN
		UPDATE dbo.Roles
		SET RoleName = @roleName
		WHERE Id = @roleId
	END
	ELSE
	BEGIN
		IF(@roleId = 0)
		BEGIN
			INSERT INTO dbo.Roles(RoleName)
			VALUES(@roleName)
			
			INSERT INTO dbo.RolePermission(RoleId,PermissionId,PermissionType)
			SELECT (SELECT Id FROM dbo.Roles r WHERE r.RoleName=@roleName),p.Id,1
			FROM @accessPermissions p
			
			INSERT INTO dbo.RolePermission(RoleId,PermissionId,PermissionType)
			SELECT (SELECT Id FROM dbo.Roles r WHERE r.RoleName=@roleName),d.Id,2
			FROM @dataPermissions d
		END
		ELSE
		BEGIN
			DELETE FROM dbo.RolePermission WHERE RoleId = @roleId
			
			INSERT INTO dbo.RolePermission(RoleId,PermissionId,PermissionType)
			SELECT @roleId,p.Id,1
			FROM @accessPermissions p
			
			INSERT INTO dbo.RolePermission(RoleId,PermissionId,PermissionType)
			SELECT @roleId,d.Id,2
			FROM @dataPermissions d
		END
	END	
END